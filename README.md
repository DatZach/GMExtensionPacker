# GameMaker Extension Packer

This tool gives you the ability to convert GameMaker Local Packages into Extensions, and Extensions into Local Packages. This allows you to develop your extensions inside of the IDE as a collection of scripts, and then package as an extension. Additionally, it enables you to modify existing extensions easily by importing them into GameMaker as scripts.

## Getting Started
Download and extract the latest [Release](https://github.com/DatZach/GMExtensionPacker/releases). If you intend to use via the command line then it's recommended that you place the files somewhere inside your `PATH` environment variable.

## Usage
### Command Line Usage
```
gmextpack <options> input [output]
Options
    -v22      Generate 2.2.5 compatible files
    -v23      Generate 2.3.1 compatible files

input         Path to a *.yymp, *.yymps, or extension *.yy file
[output]      Path to output file. OPTIONAL
    If not provided then input filename is used to generate the output name
    Output for yymp/s will be to a directory, not file
```

### Shell Usage
Drag and drop a `*.yymp`, `*.yymps`, or extension `*.yy` file onto the EXE. A console window should flash up before disappearing. If successful, the result will be present in the same directory as the file you dragged in. When used like this, a Local Package will result in an Extension `*.yy` file.

### Workflow for creating an extension
Develop the scripts for your plugin inside of a standard GameMaker project. When you're ready to create an extension select `Tools > Create Local Package` and select all the relevant scripts. Once complete, pass the file to GM Extension Packer. The resulting `.yy` file can be added to GameMaker projects by right clicking on `Extensions` and selecting `Add Existing`.

#### JSDoc
When converting to an extension the jsdoc header is parsed and used to correctly build the extension function information. At a minimum, you must document each argument and its type. Argument names, if provided will be used to build the help string for the IDE's autocomplete feature. Return should also be documented, otherwise a double will be assumed as the return value. The name of the function is determined by the script's name, not jsdoc. Additonally you can specify a special attribute `@hidden` to hide the extension function from the IDE.

Script `add`
```gml
/// @param {real} a
/// @param {real} b
/// @returns {real} result
/// @hidden

return argument0 + argument1;
```

#### Macros / Constants
When converting to an extension, any macros specified the script `<LocalPackageName>_ext_macros` will be converted to constants in the extension. If this script does not exist, or macros are declared in other scripts, then no constants will be added to the extension.

#### Initializer / Finalizer
When converting to an extension, initializers and finalizers can be specified via `gml_pragma("global", ...)` in the script `<LocalPackageName>_ext_init` and `<LocalPackageName>_ext_final`. If these scripts do not exist, then no initializer or finalizer respectively will be declared in the extension.

Script `Example_ext_init`
```gml
gml_pragma("global", "example_initialize();");
```

### Workflow for importing an extension as scripts
Run the `*.yy` extension through the tool. Then in the GameMaker IDE select `Tools > Import Local Package`, add all files and accept. If there are macros or initializers/finalizers they will be imported as scripts with the names documented above.

Some of the reasons you might want to import a GML extension like this:
 - Improved compile times. Extension functions compile slower than standard scripts.
 - Modify buggy code in extensions.

## Known Issues
 - Version 2.3 support is not yet implemented.
 - Poor error handling.

## Authors

* **Zach Reedy** - *Primary developer* - [DatZach](https://github.com/DatZach)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details