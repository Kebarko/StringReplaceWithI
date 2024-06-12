# StringReplaceWithI

## Overview

Simple text search and replace tool with optional use of an iterator.

## Requirements

`.NET 8`

## Installation

Unpack `StringReplaceWithI.zip`

## Usage

This program can search and replace strings in text just like a regular text editor. However, you can use an iterator when replacing a string. The iterator has the following pattern: `\i{init, step, sign}`, where `init` is the initial value (default `1`), `step` is the step that determines the increment or decrement of the iterator (default `1`) and `sign` is either a `+` or `-` character that determines the direction of the iteration (default `+`). This means that each parameter is optional. You can use more than one iterator in a replacement string. You can also use a regular expression to search for a string.

### Examples of iterators

`\i` - iterator with initial value of `1`, increment `1`, resulting in `1, 2, 3, ...`

`\i{5}` - iterator with initial value of `5`, increment `1`, resulting in `5, 6, 7, ...`

`\i{0, 5}` - iterator with initial value of `0`, increment `5`, resulting in `0, 5, 10, ...`

`\i{0, 5, -}` - iterator with initial value of `0`, decrement `5`, resulting in `0, -5, -10, ...`

`\i{100, 2, -}` - iterator with initial value of `100`, decrement `2`, resulting in `100, 98, 96, ...`

### Example

Original text:

> The Institute of Electrical and Electronics Engineers (`IEEE`) is a global organization dedicated to advancing technology for the benefit of humanity. `IEEE` publishes numerous journals and conferences where researchers present their latest findings. Whether it's in the field of robotics, telecommunications, or renewable energy, `IEEE` plays a pivotal role in shaping the future of technology. Joining IEEE provides access to a vast network of professionals and resources, fostering innovation and collaboration. With `IEEE`'s standards guiding technological development, the world moves towards a more connected and efficient future.

Find: `IEEE`

Replace: `IEEE\i{10, 2}`

Result:

> The Institute of Electrical and Electronics Engineers (`IEEE10`) is a global organization dedicated to advancing technology for the benefit of humanity. `IEEE12` publishes numerous journals and conferences where researchers present their latest findings. Whether it's in the field of robotics, telecommunications, or renewable energy, `IEEE14` plays a pivotal role in shaping the future of technology. Joining `IEEE16` provides access to a vast network of professionals and resources, fostering innovation and collaboration. With `IEEE18`'s standards guiding technological development, the world moves towards a more connected and efficient future.

## License

This project is licensed under the MIT License.
