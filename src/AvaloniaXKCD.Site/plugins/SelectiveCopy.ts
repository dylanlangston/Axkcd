import { copy, remove, readdir, pathExists } from 'fs-extra'
import path from 'node:path'
import type { Plugin, ResolvedConfig } from 'vite'

export interface SelectiveCopyOptions {
    source: string;
    dest: string;
}

export function selectiveCopy(options: SelectiveCopyOptions): Plugin {
    let config: ResolvedConfig;

    return {
        name: 'selective-copy',
        apply: 'build',

        configResolved(resolvedConfig) {
            config = resolvedConfig;
        },

        async closeBundle() {
            const sourceDir = path.resolve(config.root, options.source);
            const destDir = path.resolve(config.build.outDir, options.dest);

            console.log(`[selective-copy] Copying assets from ${sourceDir} to ${destDir}...`);

            try {
                await copy(sourceDir, destDir, {
                    overwrite: false,
                    dereference: true,
                    errorOnExist: false,
                });
                console.log(`[selective-copy] Asset copy complete.`);
            } catch (error) {
                this.error(`[selective-copy] Failed to copy assets: ${error}`);
            }
        },
    }
}