import { copy } from 'fs-extra'
import path from 'node:path'
import type { Plugin } from 'vite'

export interface ExternalImportsOptions {
    imports: Record<string, string | undefined>
}

export function mapExternalImports(options: ExternalImportsOptions): Plugin {
    return {
        name: 'map-external-imports',
        enforce: 'pre',
        resolveId(source: string) {
            const mapping = options.imports?.[source];
            if (mapping) {
                return { id: mapping, external: true };
            }
            return null;
        },
    }
}