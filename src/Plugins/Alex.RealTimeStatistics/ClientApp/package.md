# Package Alternatives
This file containts references for changing out parts of package.json depending on what features you want
## ESLint / Prettier Options
- See comments in eslintrc.js
- Also recommend editor.config so VS Code Prettier doesn't just pickup Code's settings
- Whole changed sections below for ease of copying and pasting

### Original ESLint / Standard
```json
    "@vue/eslint-config-standard": "^5.1.2",
    "@vue/eslint-config-typescript": "^7.0.0",
    "eslint": "^6.7.2",
    "eslint-plugin-import": "^2.20.2",
    "eslint-plugin-node": "^11.1.0",
    "eslint-plugin-promise": "^4.2.1",
    "eslint-plugin-standard": "^4.0.0",
    "eslint-plugin-vue": "^6.2.2",
    "node-sass": "^5.0.0",
```

### Prettier Alternative
```json
    "@vue/eslint-config-prettier": "^6.0.0",
    "@vue/eslint-config-typescript": "^7.0.0",
    "eslint": "^6.7.2",
    "eslint-plugin-prettier": "^3.3.1",
    "eslint-plugin-vue": "^6.2.2",
    "node-sass": "^5.0.0",
    "prettier": "^2.2.1",
```

*[Idea for package.md](https://spin.atomicobject.com/2019/05/20/document-package-json/)*
