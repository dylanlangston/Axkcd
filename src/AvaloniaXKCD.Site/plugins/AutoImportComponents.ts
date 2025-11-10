import path from 'node:path'
import { glob } from 'glob'
import type { Plugin } from 'vite'

interface Component {
    name: string;
    path: string;
}

export function autoImportComponents(): Plugin {
    const components: Component[] = glob.sync('src/components/**/*.ts').map((file: string) => ({
        name: path.basename(file, '.ts'),
        path: `/${file}`
    }));

    return {
        name: 'auto-import-components',
        transform(code, id) {
            if (id.endsWith('main.ts')) {
                const imports = components
                    .map(component => `import '${component.path}';`)
                    .join('\n');
                return `${imports}\n${code}`;
            }
        }
    }
}