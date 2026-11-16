FROM alpine:3.22

RUN apk add --no-cache ca-certificates curl jq

WORKDIR /conformance
COPY schemas/coherent.md schemas/extract.md schemas/
COPY typescript/e2e/crucible.config.json judge.config.json
COPY conformance/cases/coherent.json conformance/cases/extract.json cases/
COPY conformance/conformance.sh conformance/payload-sections.jq conformance/coherent-payload.jq conformance/extract-payload.jq conformance/reply-repair.jq ./

USER nobody
ENTRYPOINT ["/bin/sh", "/conformance/conformance.sh"]
CMD ["run"]
