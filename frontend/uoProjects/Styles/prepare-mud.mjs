import { readFileSync, writeFileSync, mkdirSync } from 'node:fs';
import { dirname } from 'node:path';
import { transform } from 'lightningcss';

const [, , source, target] = process.argv;

const isMudClass = (component) => component.type === 'class' && component.name.startsWith('mud');
const hasClass = (selector) => selector.some((component) => component.type === 'class');
const hasMudClass = (selector) => selector.some(isMudClass);

const { code } = transform({
  filename: source,
  code: readFileSync(source),
  minify: true,
  visitor: {
    Rule: {
      style(rule) {
        const selectors = rule.value.selectors;
        const helperOnly = selectors.every((selector) => hasClass(selector) && !hasMudClass(selector));
        return helperOnly ? [] : undefined;
      },
    },
  },
});

mkdirSync(dirname(target), { recursive: true });
writeFileSync(target, code);
