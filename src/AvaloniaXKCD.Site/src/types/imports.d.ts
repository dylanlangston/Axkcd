/**
 * This declaration allows TypeScript projects to import the .NET runtime script
 * from its default path '/_framework/dotnet.js'.
 * It re-exports all the types from the main './dotnet' type definition file,
 * enabling full type checking and IntelliSense for the .NET runtime API.
 */
declare module '/_framework/dotnet.js' {
  export * from './dotnet';
}