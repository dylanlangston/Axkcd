import { readFile, writeFile } from "fs/promises";
import { resolve } from "path";
import { Plugin } from "vite";
import { minify } from "html-minifier-terser";

export function copyIndexTo404AndMinify(): Plugin {
  return {
    name: 'copy-index-to-404-and-minify',
    apply: 'build',
    async writeBundle(options) {
      const outDir = options.dir;
      if (!outDir) {
        console.warn('Could not determine output directory. Skipping 404.html copy and minification.');
        return;
      }

      const indexPath = resolve(outDir, 'index.html');
      const fourOhFourPath = resolve(outDir, '404.html');

      try {
        let indexHtml = await readFile(indexPath, 'utf-8');

        const minifiedHtml = await minify(indexHtml, {
          collapseWhitespace: true,
          keepClosingSlash: true,
          removeComments: true,
          removeRedundantAttributes: true,
          removeScriptTypeAttributes: true,
          removeStyleLinkTypeAttributes: true,
          useShortDoctype: true,
          minifyCSS: true,
          minifyJS: true,
        });

        await writeFile(indexPath, minifiedHtml, 'utf-8');
        await writeFile(fourOhFourPath, minifiedHtml, 'utf-8');

      } catch (error) {
        console.error(`Could not copy and minify index.html to 404.html: ${error}`);
      }
    }
  };
}