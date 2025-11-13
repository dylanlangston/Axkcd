import type { Plugin } from 'vite';
import fs from 'node:fs/promises';
import path from 'node:path';

export function injectLatestComic(): Plugin {
    return {
        name: 'inject-latest-comic',
        transformIndexHtml: async (html) => {
            try {
                const infoPath = path.resolve(__dirname, '../../../mirror/info.0.json');
                const infoContent = await fs.readFile(infoPath, 'utf-8');
                const comicInfo = JSON.parse(infoContent);

                // Get the image path without the extension to construct 2x path
                const imgPathWithoutExt = comicInfo.img.replace(/\.[^/.]+$/, '');
                const comicImgPath = `https://dylanlangston.github.io/Axkcd/mirror/${comicInfo.img}`;
                const comic2xImgPath = `https://dylanlangston.github.io/Axkcd/mirror/${imgPathWithoutExt}_2x.png`;
                const wittyMessage = `It appears you've disabled JavaScript, likely because you're wary of running arbitrary code from a stranger's server. This suggests a healthy level of paranoia, which, coincidentally, is a prerequisite for appreciating the subtle humor in debugging logs and the existential dread of a poorly timed git push. You'll fit right in here.

However, this site, in its ongoing battle against the forces of static HTML, relies on the very scripting you've forsaken. To unlock the full experience&#8212;and to prevent the stick figures from unionizing over unfair rendering conditions&#8212;you'll need to re-enable it.

(Don't worry, we promise our code is only mostly sentient.)`;

                const noscriptContent = `<div class="card" style="text-align: center; padding: 20px; max-width: 1100px; margin: 14px;">
   <p style="white-space: pre-wrap;">${wittyMessage}</p>
   <hr/>
   <p>Here's comic #${comicInfo.num}: "<a target="_blank" rel="noopener noreferrer" href="https://xkcd.com/${comicInfo.num}" style="color: var(--text-color); text-decoration: underline;">${comicInfo.title}</a>"</p>
   <div style="display: flex; justify-content: center; align-items: center; margin: 20px auto;">
      <picture style="max-width: 100%; height: auto; display: block;">
         <source srcset="${comic2xImgPath}" media="(min-width: 200px)">
         <img src="${comicImgPath}" alt="${comicInfo.alt}" title="${comicInfo.title}" style="max-width: 100%; height: auto; display: block;">
      </picture>
   </div>
   <p style="font-style: italic; max-width: 800px; margin: 20px auto;">${comicInfo.alt}</p>
</div>
`;

                const comicUrl = `/${comicInfo.num}`;

                const openGraphTags = `<meta property="og:title" content="AvaloniaXKCD: ${comicInfo.title}" />
<meta property="og:type" content="article" />
<meta property="og:url" content="${comicUrl}" />
<meta property="og:image" content="${comicImgPath}" />
<meta property="og:description" content="${comicInfo.alt}" />
`;

                let updatedHtml = html.replace(
                    /<noscript>[\s\S]*?<\/noscript>/i,
                    `<noscript>${noscriptContent}</noscript>`
                );

                updatedHtml = updatedHtml.replace(
                    '</head>',
                    `${openGraphTags}</head>`
                );

                return updatedHtml;

            } catch (error) {
                console.error('Failed to inject latest comic:', error);
                return html;
            }
        }
    };
}