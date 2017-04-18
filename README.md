# ImageCruncher
Application to create custom .btf files (similar to .bmp) for streaming data from SD card to POV LED devices. These images need to be quickly
read and loaded, or have a small enough footprint that a microcontroller can store multiples in RAM.

The .btf image format is a work in progress, and currently just has a reduced header size, with pixels organized in column-first order.
Going forward, it will allow for 4/16/256 color schemes, to index into FastLED palettes and reduce the size of each pixel to 2/4/8 bits.
