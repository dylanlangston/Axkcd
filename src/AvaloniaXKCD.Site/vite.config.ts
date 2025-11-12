import { resolve } from 'node:path'
import { defineConfig, loadEnv } from 'vite'
import { injectVersionHeader } from './plugins/InjectVersionHeader'
import { autoImportComponents } from './plugins/AutoImportComponents'
import { selectiveCopy } from './plugins/SelectiveCopy'
import { injectLatestComic } from './plugins/InjectLatestComic'
import { copyIndexTo404AndMinify } from './plugins/CopyIndexTo404AndMinify'
import { viteSingleFile } from "vite-plugin-singlefile"
import { minifyTemplateLiterals } from 'rollup-plugin-minify-template-literals';
import { VitePWA } from 'vite-plugin-pwa'
import pkg from './package.json'
import { mapExternalImports } from './plugins/MapExternalImports'

export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '')

    return {
        define: {
            __APP_ENV__: JSON.stringify(env.APP_ENV),
            __VERSION__: pkg.version
        },
        base: './',
        server: undefined,
        appType: 'spa',
        resolve: {
            alias: [
                { find: '@', replacement: resolve(__dirname, 'src') }
            ]
        },
        plugins: [
            mapExternalImports({imports: {
                '@framework/dotnet.js': './_framework/dotnet.js'
            }}),
            injectVersionHeader(),
            minifyTemplateLiterals(),
            copyIndexTo404AndMinify(),
            autoImportComponents(),
            selectiveCopy({
                source: '../../mirror',
                dest: './mirror/'
            }),
            injectLatestComic(),
            VitePWA({
                registerType: 'autoUpdate',
                injectRegister: 'script-defer',
                pwaAssets: { disabled: false, config: true, htmlPreset: '2023', overrideManifestIcons: true },
                includeAssets: ["_framework"],
                manifest: {
                    name: 'A(valonia)XKCD',
                    id: 'axkcd',
                    short_name: 'AXKCD',
                    theme_color: '#6e7b91',
                    icons: [
                        {
                            src: 'pwa-64x64.png',
                            sizes: '64x64',
                            type: 'image/png',
                        },
                        {
                            src: 'pwa-192x192.png',
                            sizes: '192x192',
                            type: 'image/png',
                        },
                        {
                            src: 'pwa-512x512.png',
                            sizes: '512x512',
                            type: 'image/png',
                        },
                        {
                            src: 'maskable-icon-512x512.png',
                            sizes: '512x512',
                            type: 'image/png',
                            purpose: 'maskable',
                        },
                    ],
                    display: 'minimal-ui',
                    edge_side_panel: {
                        "preferred_width": 800
                    }
                },
                workbox: {
                    runtimeCaching: [
                        {
                            urlPattern: ({ url }) => url.pathname.includes('/_framework/'),
                            handler: 'NetworkFirst',
                            options: {
                                cacheName: 'dotnet-cache',
                                cacheableResponse: {
                                    statuses: [0, 200]
                                },
                                expiration: {
                                    maxEntries: 500,
                                    maxAgeSeconds: 60 * 60 * 24 * 365
                                }
                            }
                        },
                        {
                            urlPattern: ({url}) => url.pathname.match(/\.(?:png|jpg)$/),
                            handler: 'CacheFirst',
                            options: {
                                cacheName: 'image-cache',
                                cacheableResponse: {
                                    statuses: [0, 200]
                                },
                                expiration: {
                                    maxEntries: 5000,
                                    maxAgeSeconds: 60 * 60 * 24 * 365
                                }
                            }
                        },
                        {
                            urlPattern: ({ url }) => url.pathname.endsWith('info.0.json'),
                            handler: 'NetworkFirst',
                            options: {
                                cacheName: 'info-cache',
                                cacheableResponse: {
                                    statuses: [0, 200]
                                },
                                expiration: {
                                    maxEntries: 5000,
                                    maxAgeSeconds: 60 * 60 * 24 * 365
                                }
                            }
                        }
                    ]
                }
            }),
            viteSingleFile({ removeViteModuleLoader: true })
        ],
        build: {
            outDir: resolve(__dirname, '../AvaloniaXKCD.Browser/wwwroot'),
            emptyOutDir: false,
            modulePreload: true,
            sourcemap: false,
            target: 'esnext',
            rollupOptions: {
                external: [
                ],
                output: {
                }
            }
        }
    }
})