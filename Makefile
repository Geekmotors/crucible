.NOTPARALLEL:

.PHONY: e2e-core e2e-offline e2e-stub-jest e2e-stub-vitest e2e e2e-key e2e-compile e2e-build conformance conformance-image spec-goldens spec-goldens-check spec-payloads spec-payloads-check dotnet-build dotnet-test dotnet-format dotnet-live

E2E_NODE_OPTIONS ?= --experimental-vm-modules --use-system-ca
E2E_OFFLINE_NODE_OPTIONS ?= --experimental-vm-modules
CONFORMANCE_IMAGE ?= crucible-ai-conformance
CONFORMANCE_REPS ?= 3
CONFORMANCE_SUITES ?= coherent extract
CONFORMANCE_CA_BUNDLE ?=
TS_DIR = typescript
DOTNET_DIR = dotnet
DOTNET ?= dotnet
DOTNET_LIVE_PROJECT = tests/Geekmotors.Crucible.LiveTests
CONFORMANCE_HARNESS = conformance
CONFORMANCE_CONFIG ?= $(TS_DIR)/e2e/crucible.config.json
MACOS_CA_KEYCHAINS = /Library/Keychains/System.keychain /System/Library/Keychains/SystemRootCertificates.keychain

E2E_JEST = cd $(TS_DIR)/e2e && NODE_OPTIONS="$(E2E_NODE_OPTIONS)" ../node_modules/.bin/jest --config jest.config.mjs --ci
E2E_STUB_ENV = env -u CRUCIBLE_JUDGE_KEY -u CRUCIBLE_VERBOSE
E2E_STUB_JEST = cd $(TS_DIR)/e2e/stub && $(E2E_STUB_ENV) NODE_OPTIONS="$(E2E_OFFLINE_NODE_OPTIONS)" ../../node_modules/.bin/jest --config jest.config.mjs --ci --runInBand
E2E_STUB_VITEST = cd $(TS_DIR)/e2e/stub && $(E2E_STUB_ENV) ../../node_modules/.bin/vitest run --config vitest/vitest.config.ts

e2e-key:
	@if [ -z "$$(printf '%s' "$$CRUCIBLE_JUDGE_KEY" | tr -d '[:space:]')" ]; then \
		echo "e2e: CRUCIBLE_JUDGE_KEY is not set. The live smoke (e2e-core) judges against real OpenRouter; set it to your OpenRouter key, or run make e2e-offline, which needs none." >&2; \
		echo "     Export your key first, e.g.: set -a; . ./.env.dev.local; set +a" >&2; \
		exit 2; \
	fi

e2e-compile:
	cd $(TS_DIR) && npm run build
	cd $(TS_DIR) && npx tsc --noEmit -p e2e

e2e-build: e2e-key e2e-compile

e2e-core: e2e-build
	$(E2E_JEST)

e2e-stub-jest: e2e-compile
	$(E2E_STUB_JEST)

e2e-stub-vitest: e2e-compile
	$(E2E_STUB_VITEST)

e2e-offline: e2e-stub-jest e2e-stub-vitest

e2e: e2e-key e2e-offline e2e-core

spec-goldens:
	sh $(CONFORMANCE_HARNESS)/spec-goldens.sh write

spec-goldens-check:
	sh $(CONFORMANCE_HARNESS)/spec-goldens.sh check

dotnet-build:
	cd $(DOTNET_DIR) && $(DOTNET) build -warnaserror

dotnet-test: dotnet-build
	cd $(DOTNET_DIR) && $(DOTNET) test --no-build

dotnet-format:
	cd $(DOTNET_DIR) && $(DOTNET) format --verify-no-changes

dotnet-live: e2e-key
	cd $(DOTNET_DIR) && $(DOTNET) build $(DOTNET_LIVE_PROJECT) -warnaserror
	cd $(DOTNET_DIR)/$(DOTNET_LIVE_PROJECT) && $(DOTNET) run --no-build --framework net10.0 -- -explicit only -failSkips

spec-payloads: spec-goldens

spec-payloads-check: spec-goldens-check

conformance-image:
	docker build --file $(CONFORMANCE_HARNESS)/Dockerfile --tag $(CONFORMANCE_IMAGE) .

CONFORMANCE_RUN = docker run --rm --volume "$(abspath $(CONFORMANCE_CONFIG)):/conformance/judge.config.json:ro"

conformance: conformance-image
	@if [ ! -f "$(CONFORMANCE_CONFIG)" ]; then \
		echo "conformance: judge config $(CONFORMANCE_CONFIG) not found. Set CONFORMANCE_CONFIG to a crucible.config.json." >&2; \
		exit 2; \
	fi
	@key_line=$$($(CONFORMANCE_RUN) $(CONFORMANCE_IMAGE) key-env) || exit $$?; \
	set -- $$key_line; key_env=$$1; key_need=$$2; \
	key_flag=; \
	if [ -n "$$(printenv "$$key_env" | tr -d '[:space:]')" ]; then \
		key_flag="--env $$key_env"; \
	elif [ "$$key_need" = required ]; then \
		echo "conformance: $$key_env is not set. The judge config $(CONFORMANCE_CONFIG) reads its key from it." >&2; \
		echo "     Export your key first, e.g.: set -a; . ./.env.dev.local; set +a" >&2; \
		exit 2; \
	fi; \
	{ \
		if [ -n "$(CONFORMANCE_CA_BUNDLE)" ]; then cat "$(CONFORMANCE_CA_BUNDLE)"; \
		elif command -v security >/dev/null 2>&1; then security find-certificate -a -p $(MACOS_CA_KEYCHAINS); \
		fi; \
	} | $(CONFORMANCE_RUN) --interactive \
		$$key_flag \
		--env CONFORMANCE_REPS=$(CONFORMANCE_REPS) \
		--env CONFORMANCE_SUITES="$(CONFORMANCE_SUITES)" \
		--env CONFORMANCE_EXTRA_CA=/dev/stdin \
		$(CONFORMANCE_IMAGE)
