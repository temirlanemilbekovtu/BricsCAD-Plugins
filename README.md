# BricsCAD Plugins

This is a custom shared plugin for BricsCAD, built in C#, containing custom functions for modifying entities. Currently, the plugin includes a single function designed for stretching selected entities along a chosen axis.

## Features

- **Stretch Entities**: 
  - Select entities and adjust their points relative to a specified base point with a specified factor.
  - Stretching is done along a selected axis.

## Usage

1. Load the plugin into BricsCAD.
2. Select the entities you want to modify.
3. Specify the axis along which the entities should be stretched.
3. Specify the multiplier by which the entities will be stretched.
4. Choose a base point for the stretch operation.

## Requirements

- BricsCAD
- .NET Framework (compatible with the version used by BricsCAD)

## Installation

1. Clone or download this repository.
2. Load the compiled plugin into BricsCAD using the appropriate plugin loader.

## License

This project is licensed under the MIT License.
