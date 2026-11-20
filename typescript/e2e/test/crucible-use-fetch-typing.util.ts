import { crucible } from 'crucible-ai';
import type { CrucibleFetchInit, CrucibleFetchResponse } from 'crucible-ai';

class CrucibleUseFetchTyping {
  private static OK = 200;
  private static REPLY = { choices: [{ message: { content: '{"verdict":true}' } }] };

  takesTheGlobalFetch(): void {
    crucible.useFetch(globalThis.fetch);
  }

  takesAWrapperThatPassesInitOn(): void {
    crucible.useFetch((url, init) => globalThis.fetch(url, { ...init, keepalive: true }));
  }

  takesATypedWrapper(): void {
    crucible.useFetch(
      async (url: string, init: CrucibleFetchInit): Promise<CrucibleFetchResponse> =>
        globalThis.fetch(url, init),
    );
  }

  takesAFakeJudge(): void {
    crucible.useFetch(() =>
      Promise.resolve(
        new Response(JSON.stringify(CrucibleUseFetchTyping.REPLY), {
          status: CrucibleUseFetchTyping.OK,
        }),
      ),
    );
  }

  rejectsANonFunction(): void {
    // @ts-expect-error useFetch takes a fetch function
    crucible.useFetch('https://proxy.example.com');
  }
}

export { CrucibleUseFetchTyping };
