const { getDefaultConfig } = require('expo/metro-config');
const path = require('path');

const projectRoot = __dirname;
const workspaceRoot = path.resolve(projectRoot, '..');

const config = getDefaultConfig(projectRoot);

// Watch the shared package outside of the mobile/ root
config.watchFolders = [workspaceRoot];

// Resolve modules from workspace root first, then mobile/
config.resolver.nodeModulesPaths = [
  path.resolve(projectRoot, 'node_modules'),
  path.resolve(workspaceRoot, 'node_modules'),
];

// Resolve @mamvibe/shared to the local package
config.resolver.extraNodeModules = {
  '@mamvibe/shared': path.resolve(workspaceRoot, 'shared'),
};

module.exports = config;
