import { Plugin } from 'vite'
import pkg from '../package.json'

export function injectVersionHeader(): Plugin {
    return {
        name: 'inject-version-header',
        transformIndexHtml() {
            return [
                {
                    tag: 'meta',
                    attrs: {
                        name: 'version',
                        content: pkg.version
                    },
                    injectTo: 'head'
                }
            ]
        }
    }
}