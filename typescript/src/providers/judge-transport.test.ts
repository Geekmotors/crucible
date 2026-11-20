import { describe, expect, it } from '@jest/globals';
import { BaseUrl } from './test/base-url.enum.js';
import { ProviderName } from './test/provider-name.enum.js';
import { RegistryHarness } from './test/registry-harness.util.js';
import { TransportHarness } from './test/transport-harness.util.js';
import { TransportReply } from './test/transport-reply.enum.js';

describe('JudgeTransport', () => {
  it.each([
    [ProviderName.OpenRouter, {}],
    [ProviderName.OpenAi, {}],
    [ProviderName.OpenAiCompatible, { baseUrl: BaseUrl.Gateway }],
  ])('%s sends judge calls through the chosen fetch', async (provider, settings) => {
    const transport = TransportHarness.transport();
    const adapter = TransportHarness.adapter(transport, provider, settings);
    transport.use(TransportHarness.serving(TransportReply.Chosen));
    await expect(TransportHarness.reply(adapter)).resolves.toBe(TransportReply.Chosen);
  });

  it('sends judge calls through the default fetch while none is chosen', async () => {
    const transport = TransportHarness.transport();
    const adapter = TransportHarness.adapter(transport, ProviderName.OpenRouter, {});
    await expect(TransportHarness.reply(adapter)).resolves.toBe(TransportReply.Default);
  });

  it('keeps the chosen fetch when the same fetch is chosen again', async () => {
    const transport = TransportHarness.transport();
    const adapter = TransportHarness.adapter(transport, ProviderName.OpenRouter, {});
    const chosen = TransportHarness.serving(TransportReply.Chosen);
    transport.use(chosen);
    transport.use(chosen);
    await expect(TransportHarness.reply(adapter)).resolves.toBe(TransportReply.Chosen);
  });

  it('keeps the chosen fetch when the same fetch is chosen again after a test started', async () => {
    const transport = TransportHarness.transport();
    const adapter = TransportHarness.adapter(transport, ProviderName.OpenRouter, {});
    const chosen = TransportHarness.serving(TransportReply.Chosen);
    transport.use(chosen);
    transport.seal();
    transport.use(chosen);
    await expect(TransportHarness.reply(adapter)).resolves.toBe(TransportReply.Chosen);
  });

  it.each([[undefined], [null], [{}]])('choosing %p as the fetch is a usage error', (value) => {
    const transport = TransportHarness.transport();
    expect(RegistryHarness.errorKind(() => transport.use(value))).toBe('usage');
  });

  it('choosing a different fetch is a usage error', () => {
    const transport = TransportHarness.transport();
    transport.use(TransportHarness.serving(TransportReply.Chosen));
    expect(
      RegistryHarness.errorKind(() =>
        transport.use(TransportHarness.serving(TransportReply.Chosen)),
      ),
    ).toBe('usage');
  });

  it('choosing a fetch after a test started is a usage error', () => {
    const transport = TransportHarness.transport();
    transport.seal();
    expect(
      RegistryHarness.errorKind(() =>
        transport.use(TransportHarness.serving(TransportReply.Chosen)),
      ),
    ).toBe('usage');
  });
});
