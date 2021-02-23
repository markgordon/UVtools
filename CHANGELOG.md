# Changelog

## 21/02/2021 - v2.5.0

* (Add) Help - Material manager (F10): Allow to manage material stock and costs with statistic over time
* (Add) File - I printed this file (CTRL + P): Allow to select a material and consume resin from stock and print time from the loaded file

## 19/02/2021 - v2.4.9

* **(Fix) PhotonWorkshop files: (#149)**
   * CurrencySymbol to show the correct symbol and don't convert unknown values to prevent hacking
   * Set PerLayerOverride to 1 only if any layer contains modified parameters that are not shared with the globals
* **(Fix) Clipboard:**
   * Initing the clipboard for the first time was calling Undo and reseting parameters from layers with base settings
   * Undo and redo make layer parameters to reset and recalculate position z, making invalid files when using with advanced tools
* (Fix) Tool - Edit print parameters: When editing per a layer range and reopen the tool it will show the previous set values

## 18/02/2021 - v2.4.8

* (Improvement) Cache per layer and global used material for faster calculations
* (Improvement) Better internal PrintTime management
* **(Improvement) GUI:**
   * Show per layer used material percentage compared to the rest model
   * Show total of millimeters cured per layer if available
   * Show bounds and ROI in millimeters if available
   * Show display width and height below resolution if available
   * Don't split (Actions / Refresh / Save) region when resize window and keep those fixed
* **(Improvement) Calibrate - Grayscale:**
   * Add a option to convert brightness to exposure time on divisions text
   * Adjust text position to be better centered and near from the center within divisions
* (Fix) Calculate the used material with global layer height instead of calculate height from layer difference which lead to wrong values in parallel computation
* (Fix) Converting files were not setting the new file as parent for the layer manager, this affected auto convertions from SL1 and lead to crashes and bad calculations if file were not reloaded from the disk (#150, #151)
* (Fix) PositionZ rounding error when removing layers

## 17/02/2021 - v2.4.7

* (Add) Computed used material milliliters for each layer, it will dynamic change if pixels are added or subtracted
* (Add) Computed used material milliliters for whole model, it will dynamic change if pixels are added or subtracted
* (Improvement) Round cost, material ml and grams from 2 to 3 decimals
* (Improvement) Operation profiles: Allow to save and get a custom layer range instead of pre-defined ranges
* **(Improvement)** PhotonWorkshop files: (#149)
   * Fill in the display width, height and MaxZ values for the printers
   * Fill in the xy pixel size values for the printers
   * Change ResinType to PriceCurrencyDec and Add PriceCurrencySymbol
   * Change Offset1 on header to PrintTime
   * Change Offset1 on layer table as NonZeroPixelCount, the number of white pixels on the layer 
   * Fix LayerPositionZ to calculate the correct value based on each layer height and fix internal layer layer height which was been set to position z
   * Force PerLayerOverride to be always 1 after save the file
* (Fix) Actions - Remove and clone layers was selecting all layer range instead of the current layer
* (Fix) Redo last action was not getting back the layer range on some cases

## 15/02/2021 - v2.4.6

* **(Improvement) Calibration - Elephant Foot:** (#145)
   * Remove text from bottom layers to prevent islands from not adhering to plate 
   * Add a option to extrude text up to a height
* **(Improvement) Calibration - Exposure time finder:** (#144)
   * Increase the left and right margin to 10mm
   * Allow to iterate over pixel brightness and generate dimmed objects to test multiple times at once
* **(Fix) File format PWS:**
   * Some files would produce black layers if pixels are not full whites, Antialiasing level was not inherit from source
   * Antialiasing level was forced 1 and not read the value from file properties
   * Antialiasing threshold pixel math was producing the wrong pixel value
* **(Fix) Raw images (jpg, png, etc):** (#146)
   * Set layer height to be 0.01mm by default to allow the use of some tools
   * When add layers by clone or other tool it don't update layers height, positions, indexes, leading to crashes
* **(Fix) Actions - Import Layers:** (#146, #147)
   * ROI calculation error leading to not process images that can potential fit inside the volumes
   * Out-of-bounds calculation for Stack type
   * Replace type was calculating out-of-bounds calculation like Stack type when is not required to and can lead to skip images
   * Better image ROI colection for Insert and Replace types instead of capture the center most 
* (Fix) Settings window: Force a redraw on open to fix auto sizes

## 12/02/2021 - v2.4.5

* (Add) Setting: Expand and show tool descriptions by default
* (Improvement) Drag and drop a file on Main Window while hold SHIFT key will open the file under a new instance
* (Improvement) PrusaSlicer & SL1 files: Allow to set custom variables on "Material - Notes" per resin to override the "Printer - Notes" variables
    This will allow custom settings per resin, for example, if you want a higher 'lift height, lift speed, etc' on more viscous resins. (#141)
* (Change) Setting: Windows vertical margin to 60px
* (Fix) Export file was getting a "Parameter count mismatch" on some file formats (#140)
* (Fix) photon and cbddlp file formats with version 3 to never hash images
* (Fix) Windows was not geting the screen bounds from the active monitor
* (Fix) Tool windows height, vertical margin and position
* **(Fix) Exposure time finder:**
  * Text label
  * Set vertical splitter to not show decimals, int value 
  * Set vertical splitter default to 0
  * Allow vertical splitter to accept negative values
  * Optimized the default values
  * Removed similar letters from text
  * Add some symbols to text to validate overexposure
  * Decrease Features height minimum value to 0.5mm

## 09/02/2021 - v2.4.4

* (Improvement) Exposure time finder: Invert circles loop into quadrants

## 08/02/2021 - v2.4.3

* **(Add) Exposure time finder:**
  * Configurable zebra bars
  * Configurable text
  * Tune defaults values to fill the space
  * Add incremental loop circles to fill space on exposure text space
* (Change) Default vertical windows margin from 250 to 400px

## 08/02/2021 - v2.4.2

* **(Improvement) Exposure time finder:**
  * Bring back shapes
  * Diameters now represent square pixels, eg: 3 = 3x3 pixels = 9 pixels total
  * Optimized default diameters

## 07/02/2021 - v2.4.1

* **(Fix) Exposure time finder:**
  * Use a spiral square instead of configurable shapes to match the exact precise set pixels
  * Set pixels as default values
  * Optimized the pixel values to always produce a closed shape
  * Rename cylinder to hole
  * Crash when diameters are empty
  * Profiles was not saving


## 06/02/2021 - v2.4.0

* (Upgrade) EmguCV/OpenCV to v4.5.1
* (Upgrade) AvaloniaUI to 1.0
* (Improvement) GUI re-touched
* (Improvement) Make pixel editor tab to disappear when pixel editor is disabled
* (Improvement) Simplify the output filename from PrusaSlicer profiles
* (Improvement) All operations require a slicer file at constructor rather than on execute, this allow exposure the open file to the operation before run it
* (Improvement) Calibrations: Auto set "Mirror Output" if open file have MirrorDisplay set
* (Change) Tool - Redraw model/supports icon
* (Change) photon and cbddlp to use version 3 by default
* (Add) Tool - Dynamic layer height: Analyze and optimize the model with dynamic layer heights, larger angles will slice at lower layer height
        while more straight angles will slice larger layer height. (#131)
* (Add) Calibration - Exposure time finder: Generates test models with various strategies and increments to verify the best exposure time for a given layer height
* (Add) File load checks, trigger error when a file have critical errors and attempt to fix non-critical errors
  * Layers must have an valid image, otherwise trigger an error
  * Layers must have a incremental or equal position Z than it previous, otherwise trigger an error
  * If layer 0 starts at 0mm it will auto fix all layers, it will add Layer Height to the current z at every layer
* (Add) Tool - Edit print parameters: Allow set parameters to each x layers and skip n layers inside the given range.
        This allow the use of optimizations in a layer pattern, for example, to set 3s for a layer but 2.5s for the next.
* (Add) Layer height property to "Layer Data" table: Shows layer height for the slice
* (Fix) When automations applied and file is saved, it will not warn user about file overwrite for the first time save
* (Fix) Tool - Redraw model/supports: Disable apply button when no file selected
* (Fix) Tool - Infill: Lack of equality member to test if same infill profile already exists
* (Fix) Auto converted files from SL1 where clipping filename at first dot (.), now it only strips known extensions
* (Fix) SL1 encoded files wasn't generating the right information and lead to printer crash
* (Fix) PrusaSlicer printer "Anycubic Photon S" LiftSpeed was missing and contains a typo (#135)
* (Fix) PrusaSlicer profile manager wasnt marking missing profiles to be installed (#135)
* (Fix) PrusaSlicer folder search on linux to also look at %HOME%/.config/PrusaSlicer (#135, #136)
* (Fix) Operations were revised and some bug fixed, most about can't cancel the progress
* (Fix) Some typos on tooltips
* (Fix) Prevent PhotonS from enconding, it will trigger error now as this format is read-only
* **(Fix) Ctrl + Shift + Z to redo the last operation:**
  * The layer range is reseted instead of pull the used values
  * Tool - Arithmetic always disabled
  * Action - Layer import didn't generate info and always disabled

## 22/01/2021 - v2.3.2

* (Add) Settings - Automations: Change only light-off delay if value is zero (Enabled by default)
* (Fix) Calibrators: Some file formats will crash when calibration test output more layers than the dummy file
* (Fix) Undo/redo don't unlock the save function

## 19/01/2021 - v2.3.1

* (Add) Calibrator - Stress Tower: Generates a stress tower to test the printer capabilities
* (Add) PrusaSlicer printer: UVtools Prusa SL1, for SL1 owners must use this profile to be UVtools compatible when using advanced tools
* (Fix) Tool - Calculator - Optimal model tilt: Layer height wasn't get pulled from loaded file and fixed to 0.05mm
* **(Fix) FileFormats:**
  * When change a global print paramenter, it will only set this same parameter on every layer, keeping other parameters intact, it was reseting every parameter per layer to globals
  * SL1: Verify if files are malformed and if there's missing configuration files (#126)
  * CTBv3: Set EncryptionMode to 0x2000000F, this allow the use of per layer settings

## 13/01/2021 - v2.3.0

* **PrusaSlicer:**
   * **In this release is recommended to discard your printer and refresh it with uvtools updated printer or replace notes over**
   * (Add) FILEFORMAT_XXX variable to auto-convert to that file format once open in UVtools
   * (Update) Print profiles fields with new PrusaSlicer version
   * (Remove) LayerOffDelay from printer notes and use only the LightOffDelay variable instead, both were being used, to avoid redundacy LayerOffDelay was dropped. Please update your printer accordingly!
   * (Remove) FLIP_XY compability from printers
   * (Remove) AntiAlias variable from printers
* **(Add) Settings - Automations:**
   * Auto save the file after apply any automation(s)
   * Auto convert SL1 files to the target format when possible and load it back
   * Auto set the extra 'light-off delay' based on lift height and speed.
* **FileFormats:**
    * (Add) Allow all and future formats to convert between them without knowing each other (Abstraction)
    * (Add) MirrorDisplay property: If images need to be mirrored on lcd to print on the correct orientation (If available)
    * (Add) MaxPrintHeight property: The maximum Z build volume of the printer (If available)
    * (Add) XYResolution and XYResolutionUm property
    * (Change) Round all setters floats to 2 decimals
    * (Change) LightOffTime variables to LayerOffDelay
    * (Fix) Files with upper case extensions doesn't load in
* (Add) Calculator - Optimal model tilt: Calculates the optimal model tilt angle for printing and to minimize the visual layer effect
* (Add) Bottom layer count to the status bar
* (Add) ZCodex: Print paramenter light-off delay"
* (Change) Island Repair: "Remove Islands Below Equal Pixels" limit from 255 to 65535 (#124)
* **(Fix) SL1:**
    * Prevent error when bottle volume is 0
    * bool values were incorrectly parsed
    * Implement missing keys: host_type, physical_printer_settings_id and support_small_pillar_diameter_percent
* **(Fix) ZIP:**
    * Material volume was set to grams
    * Bed Y was not being set

## 10/01/2021 - v2.2.0

* (Add) FDG file format for Voxelab Printers (ezrec/uv3dp#129)
* (Add) PrusaSlicer printer: Voxelab Ceres 8.9
* (Change) Print time display to hours and minutes: 00h00m

## 07/01/2021 - v2.1.3

* (Add) PrusaSlicer printers:
   * Peopoly Phenom XXL
   * QIDI 3D ibox mono
   * Wanhao CGR Mini Mono
   * Wanhao CGR Mono
* (Add) PrusaSlicer light supports profiles
* (Add) Calibration - Elephant Foot: Mirror output
* (Add) Calibration - XYZ Accuracy: Mirror output
* (Add) Calibration - Tolerance: Mirror output
* (Add) Calibration - Grayscale: Mirror output
* (Add) Scripts on github
* (Change) Save 'Display Width' and 'Height' to calibration profiles and load them back only if file format aware from these properties
* (Fix) Tool - Morph: Set a rectangular 3x3 kernel by default
* (Fix) Tool - Blur: Set a rectangular 3x3 kernel by default
* (Fix) Calibration - Elephant Foot: Include part scale on profile text
* (Fix) MSI dont store instalation path (#121)

## 03/01/2021 - v2.1.2

* (Add) Pixel editor - Text: Preview of text operation (#120)
* (Add) Calibration - Elephant Foot: 'Part scale' factor to scale up test parts
* (Change) Allow tools text descriptions to be selectable and copied
* (Fix) Pixel editor - Text: Round font scale to avoid precision error

## 03/01/2021 - v2.1.1

* (Add) About box: Primary screen identifier and open on screen identifier
* (Add) Calibrator - External tests
* (Change) Rewrite 'Action - Import Layer(s)' to support file formats and add the followig importation types:
  * **Insert:** Insert layers. (Requires images with bounds equal or less than file resolution)
  * **Replace:** Replace layers. (Requires images with bounds equal or less than file resolution)
  * **Stack:** Stack layers content. (Requires images with bounds equal or less than file resolution)
  * **Merge:** Merge/Sum layers content. (Requires images with same resolution)
  * **Subtract:** Subtract layers content. (Requires images with same resolution)
  * **BitwiseAnd:** Perform a 'bitwise and' operation over layer pixels. (Requires images with same resolution)
  * **BitwiseOr:** Perform a 'bitwise or' operation over layer pixels. (Requires images with same resolution)
  * **BitwiseXOr:** Perform a 'bitwise xor' operation over layer pixels. (Requires images with same resolution)
* (Change) Icon for Tool - Raft Relief
* (Change) Windows and dialogs max size are now calculated to where window is opened instead of use the primary or first screen all the time

## 29/12/2020 - v2.1.0

* (Add) Tool - Redraw model/supports: Redraw the model or supports with a set brightness. This requires an extra sliced file from same object but without any supports and raft, straight to the build plate.
* (Add) Tool - Raft Relief:
    * Allow ignore a number of layer(s) to start only after that number, detault is 0
    * Allow set a pixel brightness for the operation, detault is 0
    * New "dimming" type, works like relief but instead of drill raft it set to a brightness level
* (Add) Arch-x64 package (#104)
* (Fix) A OS dependent startup crash when there's no primary screen set (#115)
* (Fix) Tool - Re height: Able to cancel the job
* (Fix) Unable to save "Calibration - Tolerance" profiles
* (Change) Core: Move all operations code from LayerManager and Layer to it own Operation* class within a Execute method (Abstraction)
* (Change) sh UVtools.sh to run independent UVtools instance first, if not found it will fallback to dotnet UVtools.dll
* (Change) Compile and zip project with WSL to keep the +x (execute) attribute for linux and unix systems
* (Change) MacOS builds are now packed as an application bundle (Auto-updater disabled for now)
* (Remove) Universal package from builds/releases

## 25/12/2020 - v2.0.0

This release bump the major version due the introduction of .NET 5.0, the discontinuation old UVtools GUI project and the new calibration wizards.
* (Upgrade) From .NET Core 3.1 to .NET 5.0
* (Upgrade) From C# 8.0 to C# 9.0
* (Upgrade) From Avalonia preview6 to rc1
    * Bug: The per layer data gets hidden and not auto height on this rc1
* (Add) Setting - General - Windows / dialogs: 
  * **Take into account the screen scale factor to limit the dialogs windows maximum size**: Due wrong information UVtools can clamp the windows maximum size when you have plenty more avaliable or when use in a secondary monitor. If is the case disable this option
  * **Horizontal limiting margin:** Limits windows and dialogs maximum width to the screen resolution less this margin
  * **Vertical limiting margin:** Limits windows and dialogs maximum height to the screen resolution less this margin
* (Add) Ctrl + Shift + Z to undo and edit the last operation (If contain a valid operation)
* (Add) Allow to deselect the current selected profile
* (Add) Allow to set a default profile to load in when open a tool
* (Add) ENTER and ESC hotkeys to message box
* (Add) Pixel dimming: Brightness percent equivalent value
* (Add) Raft relief: Allow to define supports margin independent from wall margin for the "Relief" type
* (Add) Pixel editor: Allow to adjust the remove and add pixel brightness values
* (Add) Calibration Menu:
    * **Elephant foot:** Generates test models with various strategies and increments to verify the best method/values to remove the elephant foot.
    * **XYZ Accuracy:** Generates test models with various strategies and increments to verify the XYZ accuracy.
    * **Tolerance:** Generates test models with various strategies and increments to verify the part tolerances.
    * **Grayscale:** Generates test models with various strategies and increments to verify the LED power against the grayscale levels.
* (Change) PW0, PWS, PWMX, PWMO, PWMS, PWX file formats to ignore preview validation and allow variations on the file format (#111)
* (Change) Tool - Edit print parameters: Increments from 0.01 to 0.5
* (Change) Tool - Resize: Increments from 0.01 to 0.1
* (Change) Tool - Rotate: Increments from 0.01 to 1
* (Change) Tool - Calculator: Increments from 0.01 to 0.5 and 1
* (Fix) PW0, PWS, PWMX, PWMO, PWMS, PWX file formats to replicate missing bottom properties cloned from normal properties
* (Fix) Drain holes to build plate were considered as traps, changed to be drains as when removing object resin will flow outwards
* (Fix) When unable to save the file it will change extension and not delete the temporary file
* (Fix) Pixel dimming wasn't saving all the fields on profiles
* (Fix) Prevent a rare startup crash when using demo file
* (Fix) Tool - Solifiy: Increase AA clean up threshold range, previous value wasn't solidifing when model has darker tones
* (Fix) Sanitize per layer settings, due some slicers are setting 0 at some properties that can cause problems with UVtools calculations, those values are now sanitized and set to the general value if 0
* (Fix) Update partial islands:
    * Was leaving visible issues when the result returns an empty list of new issues
    * Was jumping some modified sequential layers
    * Was not updating the issue tracker map
* (Fix) Edit print parameters was not updating the layer data table information

## 08/11/2020 - v1.4.0

* (Add) Tool - Raft relief: Relief raft by adding holes in between to reduce FEP suction, save resin and easier to remove the prints.

## 04/11/2020 - v1.3.5

* (Add) Pixel Dimming: Chamfer - Allow the number of walls pixels to be gradually varied as the operation progresses from the starting layer to the ending layer (#106)
* (Add) PrusaSlicer print profiles: 0.01, 0.02, 0.03, 0.04, 0.15, 0.2
* (Change) Morph: "Fade" to "Chamfer" naming, created profiles need redo
* (Change) Pixel Dimming: Allow start with 0px walls when using "Walls Only"
* (Change) PrusaSlicer print profiles names, reduced bottom layers and raft height
* (Remove) PrusaSlicer print profiles with 3 digit z precision (0.025 and 0.035)
* (Fix) PW0, PWS, PWMX, PWMO, PWMS, PWX file formats, where 4 offsets (16 bytes) were missing on preview image, leading to wrong table size. Previous converted files with UVtools wont open from now on, you need to reconvert them. (ezrec/uv3dp#124)
* (Fix) Unable to run Re-Height tool due a rounding problem on some cases (#101)
* (Fix) Layer preview end with exception when no per layer settings are available (SL1 case)

## 26/11/2020 - v1.3.4

* (Add) Infill: CubicDynamicLink - Alternates centers with lateral links, consume same resin as center linked and make model/infill stronger.
* (Add) Update estimate print time when modify dependent parameters (#103)
* (Add) Tool - Calculator: Old and new print time estimation (#103)
* (Fix) Print time calculation was using normal layers with bottom layer off time
* (Fix) Calculate print time based on each layer setting instead of global settings

## 25/11/2020 - v1.3.3

* (Add) Improved island detection: Combines the island and overhang detections for a better more realistic detection and to discard false-positives. (Slower)
   If enabled, and when a island is found, it will check for overhangs on that same island, if no overhang found then the island will be discarded and considered safe, otherwise it will flag as an island issue.
   Note: Overhangs settings will be used to configure the detection. Enabling Overhangs is not required for this procedure to work.
   Enabled by default.
* (Add) More information on the About box: Operative system and architecture, framework, processor count and screens
* (Fix) Overhangs: Include islands when detecting overhangs were not skip when found a island
* (Fix) Decode CWS from Wanhao Workshop fails on number of slices (#102)

## 19/11/2020 - v1.3.2

* (Add) Tools: Warn where layer preview is critical for use the tool, must disable layer rotation first (#100)
* (Add) CWS: Bottom lift speed property
* (Add) CWS: Support Wanhao Workshop CWX and Wanhao Creation Workshop file types (#98)
* (Add) CWS: Split format into virtual extensions (.cws, .rgb.cws, .xml.cws) to support diferent file formats and diferent printers under same main .cws extensions. That will affect file converts only to let UVtools know what type of encoding to use. Load and save a xxx.cws file will always auto decode/encode the file for the correct target format no matter the extension.
* (Improvement) CWS: It no longer search for a specific filename in the zip file, instead it look for extension to get the files to ensure it always found them no matter the file name system
* (Fix) CWS: When "Save as" the file were generating sub files with .cws extension, eg: filename0001.cws.png
* (Change) Allow read empty layers without error from Anycubic files (PWS, PW0, PWxx) due a bug on slicer software under macOS

## 16/11/2020 - v1.3.1

* (Add) File format: PWX (AnyCubic Photon X) (#93)
* (Add) File format: PWMO (AnyCubic Photon Mono) (#93)
* (Add) File format: PWMS (AnyCubic Photon Mono SE) (#93)
* (Add) PrusaSlicer printer: AnyCubic Photon X
* (Add) PrusaSlicer printer: AnyCubic Photon Mono
* (Add) PrusaSlicer printer: AnyCubic Photon Mono SE
* (Add) PrusaSlicer printer: AnyCubic Photon Mono X
* (Change) "Save as" file filter dialog with better file extension description
* (Fix) Tool - Infill: Allow save profiles
* (Fix) Material cost was showing as ml instead of currency

## 14/11/2020 - v1.3.0

* (Add) Changelog description to the new version update dialog
* (Add) Tool - Infill: Proper configurable infills
* (Add) Pixel area as "px�" to the layer bounds and ROI at layer bottom information bar
* (Add) Pixel dimming: Alternate pattern every x layers
* (Add) Pixel dimming: Lattice infill
* (Add) Solidify: Required minimum/maximum area to solidify found areas (Default values will produce the old behaviour)
* (Add) Issues: Allow to hide and ignore selected issues
* (Add) Issue - Touch boundary: Allow to configure Left, Top, Right, Bottom margins in pixels, defaults to 5px (#94)
* (Add) UVJ: Allow convert to another formats (#96)
* (Add) Setters to some internal Core properties for more abstraction
* (Improvement) Issue - Touch boundary: Only check boundary pixels if layer bounds overlap the set margins, otherwise, it will not waste cycles on check individual rows of pixels when not need to
* (Change) Place .ctb extension show first than .cbddlp due more popular this days
* (Change) Pixel dimming: Text "Borders" to "Walls"
* (Change) Issues: Remove "Remove" text from button, keep only the icon to free up space
* (Change) Ungroup extensions on "convert to" menu (#97)
* (Fix) Issues: Detect button has a incorrect "save" icon
* (Fix) SL1: Increase NumSlow property limit
* (Fix) UVJ: not decoding nor showing preview images
* (Fix) "Convert to" menu shows same options than previous loaded file when current file dont support convertions (#96)
* (Fix) Hides "Convert to" menu when unable to convert to another format (#96)
* (Fix) Program crash when demo file is disabled and tries to load a file in
* (Fix) Rare crash on startup when mouse dont move in startup period and user types a key in meanwhile
* (Fix) On a slow startup on progress window it will show "Decoded layers" as default text, changed to "Initializing"

## 08/11/2020 - v1.2.1

* (Add) IsModified property to current layer information, indicates if layer have unsaved changes
* (Add) Splitter between preview image and properties to resize the vertical space between that two controls
* (Fix) Unable to save file with made modifications, layer IsModified property were lost when entering on clipboard
* (Fix) After made a modification clipboard tries to restores that same modification (Redundant)
* (Fix) Current layer data doesn't refresh when refreshing current layer, made changes not to show in
* (Fix) Hides not supported properties from current layer data given the file format

## 07/11/2020 - v1.2.0

* (Add) RAM usage on title bar
* (Add) Clipboard manager: Undo (Ctrl + Z) and Redo (Ctrl + Y) modifications (Memory optimized)
* (Add) Current layer properties on information tab
* (Fix) Long windows with system zoom bigger than 100% were being hidden and overflow (#90)
* (Fix) Do not recompute issues nor properties nor reshow layer if operation is cancelled or failed

## 05/11/2020 - v1.1.3

* (Add) Auto-updater: When a new version is detected UVtools still show the same green button at top, 
on click, it will prompt for auto or manual update.
On Linux and Mac the script will kill all UVtools instances and auto-upgrade.
On Windows the user must close all instances and continue with the shown MSI installation
* (Add) Tool profiles: Create and remove named presets for some tools
* (Add) Event handler for handling non-UI thread exceptions
* (Fix) Mac: File - Open in a new window was not working
* (Fix) Tool - Rotate: Allow negative angles
* (Fix) Tool - Rotate: The operation was inverting the angle
* (Fix) Tools: Select normal layers can crash the program with small files with low layer count, eg: 3 layers total

## 02/11/2020 - v1.1.2

* (Add) Program start elapsed seconds on Log
* (Add) Lift heights @ speeds, retract speed, light-off information to status bar
* (Fix) Per layer settings are being lost when doing operations via tools that changes the layer count
* (Fix) Current layer height mm was being calculated instead of showing the stored position Z value (For hacked files)
* (Fix) Zip: By using hacked gcodes were possible to do a lift sequence without returning back to Z layer position
* (Fix) ZCodex: Read per layer lift height/speed, retract speed and pwm from GCode
* (Fix) Status bar, layer top and bottom bar: Break content down for the next line if window size overlaps the controls
* (Fix) Status bar: Make right buttons same height as left buttons
* (Improvement) CWS: Better gcode parser for decoding
* (Change) GCodes: Cure commands (Light-on/Cure time/Light-off) are only exposed when exposure time and pwm are present and greater than 0 [Safe guard]
* (Change) Zip: If only one G0 command found per layer, it will be associated to the cure z position (No lift height)
* (Change) Merged bottom/normal exposure times on status bar
* (Change) Tabs: Change controls spacing from 5 to 2 for better looking
* (Change) Deploy UVtools self-contained per platform specific: (#89)
  * Platform optimized 
  * Reduced the package size
  * Includes .NET Core assemblies and dont require the installation of .NET Core
  * Can execute UVtools by double click on "UVtools" file or via "./UVtools" on terminal
  * **Naming:** UVtools_[os]-[architecture]_v[version].zip
  * **"universal"** zip file that includes the portable version, os and architecture independent but requires dotnet to run, these build were used in all previous versions

## 01/11/2020 - v1.1.1

* (Fix) PHZ, PWS, LGS, SL1 and ZCodex per layer settings and implement missing properties on decode
* (Fix) LGS and PHZ Zip wasn't setting the position z per layer
* (Fix) Add missing ctb v3 per layer settings on edit parameters window
* (Fix) PWS per layer settings internal LiftSpeed was calculating in mm/min, changed to mm/sec

## 01/11/2020 - v1.1.0

* (Add) photons file format (Read-only)
* (Add) Allow mouse scroll wheel on layer slider and issue tracker to change layers (#81)
* (Add) Menu - Help - Open settings folder: To open user settings folder
* (Add) When a file doesn't have a print time field or it's 0, UVtools calculate the approximate time based on parameters
* (Add) Per layer settings override on UVtools layer core
* (Add) Tool - Edit print parameters: Allow change per layer settings on a layer range
* (Add) Tool Window - Layer range synchronization and lock for single layer navigation (Checkbox)
* (Add) Tool Window - Change the start layer index on range will also change the layer image on background
* (Improvement) Adapt every file format to accept per layer settings where possible
* (Improvement) Better gcode checks and per layer settings parses
* (Change) When converting to CTB, version 3 of the file will be used instead of version 2
* (Change) When converting to photon or cbddlp, version 2 of the file will be used
* (Change) New logo, thanks to (Vinicius Silva @photonsters)
* (Fix) MSI installer was creating multiple entries/uninstallers on windows Apps and Features (#79)
* (Fix) Release builder script (CreateRelease.WPF.ps1): Replace backslash with shash for zip releases (#82)
* (Fix) CWS file reader when come from Chitubox (#84)
* (Fix) CWS was introducing a big delay after each layer, LiftHeight was being used 2 times instead of LiftSpeed (#85)
* (Fix) CWS fix Build Direction property name, was lacking a whitespace
* (Fix) Layer bounds was being show for empty layers on 0x0 position with 1px wide
* (Fix) Empty layers caused miscalculation of print volume bounds
* (Fix) Recalculate GCode didn't unlock save button
* (Fix) Tool - Calculator - Light-Off Delay: Wasn't calculating bottom layers
* (Change) Drop a digit from program version for simplicity, now: MAJOR.MINOR.PATCH 
  * **Major:** new UI, lots of new features, conceptual change, incompatible API changes, etc.
  * **Minor:** add functionality in a backwards-compatible manner
  * **Patch:** backwards-compatible bug fixes
* (Upgrade) Avalonia framework to preview6

## 23/10/2020 - v1.0.0.2

* (Fix) ROI selection button on bottom was always disabled even when a region is selected
* (Fix) Settings - Issues- "Pixel intensity threshold" defaults to 0, but can't be set back to 0 after change (minimum is 1). (#78)
* (Fix) Settings - Issues - "Supporting safe pixels..." is present twice (#78)
* (Fix) Settings - Layer repair - Empty layers / Resin traps texts are swapped in the settings window (#78)

## 23/10/2020 - v1.0.0.1

* (Change) Checked and click buttons highlight color for better distinguish
* (Fix) Move user settings to LocalUser folder to allow save without run as admin
* (Fix) Save button for print parameters were invisible

## 22/10/2020 - v1.0.0.0

* (Add) Multi-OS with Linux and MacOS support
* (Add) Themes support
* (Add) Fullscreen support (F11)
* (Change) GUI was rewritten from Windows Forms to WPF Avalonia, C#
* (Improvement) GUI is now scalable
* (Fix) Some bug found and fixed during convertion

## 14/10/2020 - v0.8.6.0

* (Change) Island detection system:
  * **Before**: A island is consider safe by just have a static amount of pixels, this mean it's possible to have a mass with 100000px supported by only 10px (If safe pixels are configured to this value), so there's no relation with island size and it supporting size. This leads to a big problem and not detecting some potential/unsafe islands.
  * **Now:** Instead of a static number of safe pixels, now there's a multiplier value, which will multiply the island total pixels per the multiplier, the supporting pixels count must be higher than the result of the multiplication.
    *  **Formula:** Supporting pixels >= Island pixels * multiplier
    *  **Example:** Multiplier of 0.25, an island with 1000px * 0.25 = 250px, so this island will not be considered if below exists at least 250px to support it, otherwise will be flagged as an island.
    *  **Notes:** This is a much more fair system but still not optimal, bridges and big planes with micro supports can trigger false islands. While this is a improvement over old system it's not perfect and you probably will have islands which you must ignore. Renember that you not have to clear out the issue list! Simply step over and ignore the issues you think are false-positives.

## 14/10/2020 - v0.8.5.0

* (Add) Tool - Calculator: Convert millimeters to pixels
* (Add) Tool - Calculator: Find the optimal "Ligth-Off Delay"
* (Add) Internal abstraction of display size to all file formats
* (Add) Default demo file that loads on startup when no file is specified (this can be disable/enabled on settings)

## 13/10/2020 - v0.8.4.3

* (Add) Tool - Layer repair: Allow remove islands recursively (#74)
* (Fix) Pixel Editor: Cursor dimentions when using brushes with thickness > 1 (#73)

## 10/10/2020 - v0.8.4.2

* (Fix) pws and pw0: Error when try to save or copy to clipboard the slicer information / properties
* (Fix) photon, ctb, cbbdlp, phz, pws, pw0: Rare cases were decoding image generate noise and malformed image
* (Fix) Rare cases where manipulation of images generate areas with noise

## 10/10/2020 - v0.8.4.1

* (Add) Tool - Modify print parameters: Value unit to confirmation text
* (Change) Tool - Modify print parameters: Maximum allowed exposure times from 255s to 1000s (#69)
* (Change) On operations, instead of partial backup a whole backup is made, this allow cancel operations which changes layer count and other structure changes
* (Improvement) PrusaSlicer profile manager: Files are now checked against checksum instead write time to prevent trigger an false update when using MSI installer
* (Fix) Tool - Layer Import: Allow cancel operation
* (Fix) Tool - Layer Import: When importing layers that increase the total layer count of the file program stays forever on progress
* (Fix) Tool - Layer Clone: Layer information was the same as heights, fixed to show the result of operation in layers
* (Fix) Tool - Pattern: Unable to use an anchor

## 01/10/2020 - v0.8.4.0

* (Add) Tool: Arithmetic operations
* (Add) Allow convert chitubox zip to cbddlp, ctb, photon, phz, pws, pw0, cws, zcodex
* (Add) When using filenames containing "bene4_mono" and when converting to cws it will use the GRAY2RGB encoding (#67)
* (Add) Hint on how to use layer re-height tool when it fails to launch
* (Add) PrusaSlicer Printer: Creality LD-006
* (Add) PrusaSlicer Printer: EPAX E6 Mono
* (Add) PrusaSlicer Printer: EPAX E10 Mono
* (Add) PrusaSlicer Printer: EPAX X1K 2K Mono
* (Add) PrusaSlicer Printer: Elegoo Mars C
* (Add) PrusaSlicer Printer: Longer 3D Orange4K
* (Add) PrusaSlicer Printer: Phrozen Shuffle XL Lite
* (Add) PrusaSlicer Printer: Phrozen Shuffle 16
* (Add) PrusaSlicer Printer: Phrozen Sonic 4K
* (Add) PrusaSlicer Printer: Phrozen Sonic Mighty 4K
* (Add) PrusaSlicer Printer: Voxelab Proxima
* (Add) PrusaSlicer Printer: QIDI S-Box
* (Fix) PrusaSlicer Printer: Elegoo Saturn - name and resolution
* (Fix) PrusaSlicer Printer: AnyCubic Photon S - display width/height
* (Fix) PrusaSlicer Printer: Epax X10 4K Mono - Y Resolution
* (Fix) PrusaSlicer Printer: EPAX X133 4K Mono - display width/height
* (Fix) PrusaSlicer Printer: Phrozen Shuffle Lite - display width/height
* (Fix) All PrusaSlicer Printers were reviewed and some bugs were fixed
* (Fix) Chitubox 3D preview when use files converted with UVtools (#68)
* (Fix) Overhangs: False-positives when previous or current layer has draker pixels, it now threshold pixels before proceed (#64)
* (Change) Tools: Placed "Threshold" menu item after "Morph"

## 30/09/2020 - v0.8.3.0

* (Add) Issue: Overhangs - Detects potential overhangs on layers (#64)
* (Add) PrusaSlicer Printer: Phrozen Sonic Mini 4K
* (Improvement) CWS: Allow read files with "slice*" filenames as content (#67)
* (Improvement) Allow convert chitubox files to CWS Bene4 Mono printer, must configure a printer containing "Bene4 Mono" name on Chitubox (#67)
* (Improvement) Edit print parameters: Show changes on confirm dialog
* (Improvement) Edit print parameters: Dedicated reset button hides when value is unchanged
* (Improvement) More detailed descriptions on error messages
* (Fix) Some islands wont remove from list when many selected and click remove
* (Fix) Extract: Use trail zeros to layer filenames
* (Fix) MSI installer not creating shortcuts (#66)

## 22/09/2020 - v0.8.2.4

* (Add) Layer Importer: Option to merge images
* (Improvement) Layer difference computation time, faster render

## 19/09/2020 - v0.8.2.3

* (Add) Tooltip for next and previous layer buttons with associated shortcut (#61)
* (Add) Pixel Editor: Erase drawing edits while hold Control (#63)
* (Add) Pixel Editor: When using diameters larger than 1px and when possible the cursor will show the associated drawing preview (#63)
* (Fix) Pixel Editor: Area px<sup>2</sup> to Diameter px (#63)
* (Fix) LGS: Some plugins and slicers use XY resolution information, while others are swapped, a auto swap will be performed when required (#59)
* (Fix) Global hotkeys prevent user from typing that key on controls (#62)

## 16/09/2020 - v0.8.2.2

* (Add) Support for PHZ zip files when renamed to .zip
* (Fix) Tools - Move and Pattern: When not selecting a ROI will draw black layers
* (Fix) Tool - Move: When making a cut move and move to a overlap zone it will blackout the source rectangle
* (Fix) ZIP: Allow to cancel on gather layers stage
* (Fix) ZIP: Thumbnails not showing nor saving

## 14/09/2020 - v0.8.2.1

* (Improvement) When unable to convert a format from SL1 to other, advice users to check used printer on PrusaSlicer (#60)
* (Improvement) Information on "Install profiles on PrusaSlicer" (#60)
* (Fix) LGS: Change resolution tool was defining wrong Y
* (Fix) ctb and pws: Renders a bad file after save, this was introduced with cancelled saves feature
* (Fix) When cancel a file convertion, it now deletes the target file

## 13/09/2020 - v0.8.2.0

* (Add) Layer status bar: Button with ROI - Click to zoom in region | Click + shift to clear roi
* (Add) Setting: Allow the layer overlay tooltips for select issues, ROI, and edit pixel mode to be hidden (#51)
* (Add) Setting: Allow change layer tooltip overlay color and opacity
* (Add) Global print properties on formats for more internal abstraction
* (Improvement) Print properties performance internal code with abstraction
* (Change) Layer status bar: Bounds text to button - Click to zoom in region
* (Change) Layer status bar: Pixel picker text to button - Click to center in point
* (Change) Layer status bar: Resolution text to button - Click to zoom to fit
* (Change) Customized cursor for Pixel Edit mode (#51)
* (Change) Layer overlay tooltip is now semi-transparent
* (Change) File - Save As is always available (#56)
* (Fix) File - Save when cancelled no longer keep a invalid file, old restored (#54)
* (Fix) File - Save As when cancelled no longer keep a invalid file, that will be deleted (#54, #55)
* (Fix) When a operation is cancelled affected layers will revert to the original form (#57)
* (Fix) Misc. text cleanup (#52, #53, #58)

## 12/09/2020 - v0.8.1.0

* (Add) Tools can now run inside a ROI (#49)
* (Add) Layer preview: Hold-Shift + Left-drag to select an ROI (Region of interest) on image, that region will be used instead of whole image when running some tools
* (Add) Layer preview: Hold-Shift + Hold-Alt + Left-drag to select and auto adjust the ROI to the contained objects, that region will be used instead of whole image when running some tools
* (Add) Layer preview: Hold-Shift + Right-click on a object to select its bounding area, that region will be used instead of whole image when running some tools
* (Add) Layer preview: ESC key to clear ROI
* (Add) Layer preview: Overlay text with hints for current action
* (Add) Tool - Move: Now possible to do a copy move instead of a cut move
* (Add) Arrow wait cursor to progress loadings
* (Change) Layer preview: Hold-Shift key to select issues and pick pixel position/brightness changed to Hold-Control key
* (Change) Layer preview: Shift+click combination to zoom-in changed to Alt+click
* (Fix) CTB v3: Bad file when re-encoding

## 11/09/2020 - v0.8.0.0

* (Add) LGS and LGS30 file format for Longer Orange 10 and 30 (ezrec/uv3dp#105)
* (Add) CWS: Support the GRAY2RGB and RBG2GRAY encoding for Bene Mono
* (Add) PrusaSlicer Printer: Nova Bene4 Mono
* (Add) PrusaSlicer Printer: Longer Orange 10
* (Add) PrusaSlicer Printer: Longer Orange 30
* (Add) Layer importer tool (#37)
* (Add) Settings & Issues: Enable or disable Empty Layers
* (Add) Layer issue Z map paired with layer navigation tracker bar
* (Add) Setting: Pixel editor can be configured to exit after each apply operation (#45)
* (Add) More abstraction on GUI and operations
* (Add) Verbose log - More a developer feature to cath bugs
* (Improvement) Redesign tools and mutator windows
* (Improvement) Erode, dilate, gap closing and noise removal converted into one window (Morph model)
* (Improvement) Convert add edit parameters into one tool window, edit all at once now
* (Improvement) Some edit parameters will trigger an error if outside the min/max limit
* (Improvement) Change some edit parameters to have decimals
* (Improvement) Kernel option on some mutators is now hidden by default
* (Improvement) When zoom into issue or drawing now it checks bounds of zoom rectangle and only performs ZoomToFit is it will be larger then the viewPort after zoom. Otherwise, it will zoom to the fixed zoom level (Auto zoom to region setting dropped as merged into this) (#42)
* (Improvement) Layer and Issues Repair: Detailed description and warning text in this dialog has been moved from main form into tooltips. It's useful information for new users, but not needed to be visible each time repair is run.
* (Improvement) Tool - Flip: Better performance on "make copy"
* (Improvement) Tool - Rotate: Disallow operation when selecting an angle of -360, 0 and 360
* (Improvement) Shortcuts: + and - to go up and down on layers were change to W and S keys. Reason: + and - are bound to zoom and can lead to problems
Less frequently used settings for gap and noise removal iterations have been moved to an advanced settings group that is hidden by default, and can be shown if changes in those settings is desired. For many users, those advanced settings can be left on default and never adjusted. (#43)
* (Change) Tool - Rotate - icon
* (Upgrade) OpenCV from 4.2 to 4.3
* (Upgrade) BinarySerializer from 8.5.2 to 8.5.3
* (Remove) Menu - Tools - Layer Removal and Layer clone for redudancy they now home at layer preview toolbar under "Actions" dropdown button
* (Fix) CWS: Add missing Platform X,Y,Z size when converting from SL1
* (Fix) CWS: Invert XY resolution when converting from SL1
* (Fix) Layer Preview: When selecting issues using SHIFT in the layer preview, the selected issue doesn't update in the issue list until after shift is released and slow operation
* (Fix) PrusaSlicer Printer: Kelant S400 Y Resolution from 1440 to 1600 and default slice settings, FLIP_XY removed, portait mode to landscape
* (Fix) Layer Clone window title was set to Pattern
* (Fix) CTB: Add support for CTB v3 (ezrec/uv3dp#97, #36)
* (Fix) SL1: Bottle volume doesn't accept decimal numbers
* (Fix) Tool - Change resolution: Confirmation text was set to remove layers
* (Fix) Fade iteration now working as expected
* (Fix) Pattern: When select big margins and cols/rows it triggers an error because value hits the maximum variable size
* (Fix) Mask: A crash when check "Invert" when mask is not loaded
* (Fix) Some text and phrases

## 04/09/2020 - v0.7.0.0

* (Add) "Rebuild GCode" button
* (Add) Issues: Touching Bounds and Empty Layers to the detect button
* (Add) Mutator - Pixel Dimming: Dims only the borders (Suggested by Marco Borzacconi)
* (Add) Mutator - Pixel Dimming: "Solid" button to set brightness only
* (Add) Issue Highlighting
  * Issues selected from the issue List View are now painted in an alternate configurable highlight color to distinguish them from non-selected issues.
  * Issues are now made active as soon as they are selected in the issue list, so single-click or arrow keys can now be used to select and issue. Double-click is no longer required.
  * Multi-select is supported. All selected issues on the currently visible layer will be highlighted with the appropriate highlight color.
  * When an issue is selected, if it is already visible in the layer preview, it will be highlighted, but not moved. If an issue is not visible when selected, it's layer will be made active (in necessary) and it will be centered in the layer preview to make it visible.
  * Issues can be selected directly from layer preview by double clicking or SHIFT+Left click on it (Hand mouse icon), also will be highlighted on issue list (This will not work while on pixel editor mode)
* (Add) Edit Pixel Operation Highlighting
  * Similar to issue highlighting, pending operations in the pixel edit view will be highlighted in an alternate configurable color when they are selected from the operations List View, including multi-select support.
  * Unlike issue highlighting, when an operation is selected from the List View, it will always be centered in the layer preview window, even if it is already visible on screen. A future update could be smarter about this and handle operations similar to issues (determining bounds of operations is a bit more involved than determining bounds of an issue).
* (Add) Crosshair Support
  * Cross-hairs can now be displayed to identify the exact location of each selected issue in the layer preview window. This is particularly beneficial at lower zoom levels to identify where issues are located within the overall layer.
  * Multi-select is supported, so selecting multiple issues will render multiple cross-hairs, one per issue.
  * Cross-hairs can be enabled/disabled on-demand using a tool strip button next to the issues button.
  * Cross-hairs can be configured to automatically fade at a specific zoom level, so that they are visible when zoomed-out, but disappear when zoomed in and issue highlighting is more obvious. The Zoom-level at which the fade occurs is configurable in settings.
  * Cross-hairs are visible in Pixel Edit mode, but they are linked to selected issues in the issues tab, not selected pixel operations in the pixel edit tab. Cross-hairs will automatically fade when an add/remove operation is initiated (via SHIFT key).
* (Add) Configurable auto-zoom level support
  * The zoom level used for auto-zoom operations is now configurable. It can be changed at any time by zooming to the desired level in the layer preview and double-clicking or CTRL-clicking the middle mouse button.
  * The currently selected auto-zoom level is indicated by a "lock" icon that appears next to the current zoom level indicator in the tool strip.
  * The default auto-zoom level (used on startup) can be configured in settings.
* (Add) Mouse-Based Navigation updates for the issue list, layer preview and pixel edit mode.
  * Issue List
     * Single Left or Right click now selects an issue from the issues list. If auto-zoom is enabled, the issue will also be centered and zoomed. Holding ALT will invert the configured behavior in your settings. With these navigation updates, leaving auto-zoom disabled in settings is recommended (and is now the new default).
     * Double-Left click or CTRL-Left-click on an issue in the issue list will zoom in on that specific issue.
     * Double-Right click or CTRL-Right-Click on any issue will zoom to fit either the build plate or the print bounds, depending on your settings. Holding ALT during the click operation will perform the inverse zoom action from what is configured in your settings(zoom plate vs zoom print bounds).
     * The Prev/Next buttons at the top of the Layer Preview will now auto-repeat if held down (similar to the layer scroll bar).
  * Layer Preview
     * Clicking in the Layer Preview window will allow you to grab and pan the image (unchanged behavior)
     * Double-Left clicking or CTRL-click on any point within the Layer Preview window will zoom in on that specific point using the locked auto-zoom level.
     * Double-Right click or CTRL-click in the layer preview will zoom-to-fit. Same behavior as double-left-click on an issue in the issue list.
     * Hold middle mouse button for 1 second will set the auto-zoom-level to the current zoom level.
     * Mouse wheel scroll behavior is unchanged (wheel scrolls in/out)
  * Pixel Edit Mode
     * Single click left or right in the pixel operation list view will now select an operation. Double click does the same (advanced zoom operations described for issue list are not currently supported from the operation list).
     * When Pixel Edit Mode is active, mouse operations in the Layer Preview area generally behave the same as described in the Layer Preview section above, including pan and double-click zoom in/out.
     * Pressing the SHIFT key in layer edit mode activates the ability to perform add/remove operations, while shift is pressed the cursor icon changes to a cross-hair, and add/remove operations can be performed. In this mode, pan and double-click zoom operations are disabled. Releasing the shift key will end add/remove mode and restore pan/zoom functions.
     * Shift-Left-Click will perform an add operations (add pixel, text, etc).
     * Shift-Right-Click will perform a remove operation (remove pixel, etc).
* (Change) Mouse coordinates on status bar now only change when SHIFT key is press, this allow to lock a position for debug
* (Remove) Confirmation for detect issues as they can now be cancelled
* (Fix) When next layer or previous layer button got disabled while pressing it get stuck
* (Fix) Partial island detection wasn't checking next layer as it should
* (Fix) chitubox: Keep some original values when read from chitubox sliced files
* (Fix) chitubox: Preview thumbnails to respect order and size names
* (Fix) Settings: Reset settings triggers a upgrade from previous version when relaunch UVtools and bring that same values
* (Fix) Issues: Touching bounds only calculate when resin traps are active
* Notes: This release is the combination of the following pull requests: #26, #27, #28, #29, #30, #31, #32, #33 (Thanks to Bryce Yancey)

## 27/08/2020 - v0.6.7.1

* (Add) Menu - Help - Benchmark: Run benchmark test to measure system performance 
* (Fix) Properties listview trigger an error when there are no groups to show
* (Fix) Elfin: "(Number of Slices = x)" to ";Number of Slices = x" (#24)

## 21/08/2020 - v0.6.7.0

* (Add) Tool: Layer Clone
* (Add) Mutator: Mask
* (Add) Mutator - Pixel Dimming: "Strips" pattern
* (Remove) Bottom progress bar

## 17/08/2020 - v0.6.6.1

* (Add) Elapsed time to the Log list
* (Add) Setting - Issues - Islands: Allow diagonal bonds with default to false (#22, #23)
* (Change) Tool - Repair Layers: Allow set both iterations to 0 to skip closing and opening operations and allow remove islands independently
* (Change) Title - file open time from miliseconds to seconds
* (Improvement) Tool - Repair Layers: Layer image will only read/save if required and if current layer got modified
* (Fix) Setting - Issues - Islands: "Pixels below this value will turn black, otherwise white" (Threshold) was not using the set value and was forcing 1
* (Fix) Remove duplicated log for repair layers and issues

## 11/08/2020 - v0.6.6.0

* (Add) Pixel Editor: Eraser - Right click over a white pixel to remove it whole linked area (Fill with black) (#7)
* (Add) Pixel Editor: Parallel layer image save when apply modifications 
* (Add) GCode: Save to clipboard
* (Change) Issues Repair: Default noise removal iterations to 0
* (Fix) Edit: Remove decimal plates for integer properties
* (Fix) cws: Exposure time was in seconds, changed to ms (#17)
* (Fix) cws: Calculate blanking time (#17)
* (Fix) cws: Edit LiftHeight and Exposure Time was enforcing integer number
* (Fix) cws: GCode extra space between slices
* (Fix) cws and zcodex: Precision errors on retract height

## 08/08/2020 - v0.6.5.0

* (Add) Mutators: Custom kernels, auto kernels and anchor where applicable
* (Add) Mutator - Blur: Box Blur
* (Add) Mutator - Blur: Filter2D
* (Improvement) Mutator: Group all blurs into one window
* (Fix) Mutators: Sample images was gone
* (Fix) Mutator - Solidify: Remove the disabled input box
* (Fix) Mutator - Pixel Dimming: Disable word wrap on pattern text box

## 06/08/2020 - v0.6.4.3

* (Add) Pixel Editor - Supports and Drain holes: AntiAliasing
* (Add) Pixel Editor - Drawing: Line type and defaults to AntiAliasing
* (Add) Pixel Editor - Drawing: Line thickness to allow hollow shapes
* (Add) Pixel Editor - Drawing: Layer depth, to add pixels at multiple layers at once
* (Add) Pixel Editor: Text writing (#7)

## 05/08/2020 - v0.6.4.2

* (Add) Hold "ALT" key when double clicking over items to invert AutoZoom setting, prevent or do zoom in issues or pixels, this will behave as !AutoZoom as long key is held
* (Improvement) Partial island update speed, huge boost performance over large files

## 04/08/2020 - v0.6.4.1

* (Add) Partial update islands from current working layer and next layer when using pixel editor or island remove
* (Add) Setting: To enable or disable partial update islands
* (Change) Properties, Issues, Pixel Editor: ListView upgraded to a FastObjectListView, resulting in faster renders, sorting capabilities, column order, groups with counter, selection, hot tracking, filtering and empty list message
* (Change) Log: ObjectListView upgraded to a FastObjectListView
* (Change) Bunch of icons

## 30/07/2020 - v0.6.4.0

* (Add) Tool: Change resolution
* (Add) Log: Track every action you do on the program

## 28/07/2020 - v0.6.3.4

* (Add) Mutator: Threshold pixels
* (Change) Mutator: PyrDownUp - Name to "Big Blur" and add better description of the effect
* (Change) Mutator: SmoothMedian - Better description
* (Change) Mutator: SmoothGaussian - Better description
* (Fix) Tool: Layer Re-Height - When go lower heights the pixels count per layer statistics are lost
* (Fix) "Pixel Edit" has the old tooltip text (#14)
* (Fix) Readme: Text fixes (#14)

## 26/07/2020 - v0.6.3.3

* (Add) Allow to save properties to clipboard
* (Add) Tool: Layer Repair - Allow remove islands below or equal to a pixel count (Suggested by: Nicholas Taylor)
* (Add) Issues: Allow sort columns by click them (Suggested by: Nicholas Taylor)
* (Improvement) Tool: Pattern - Prevent open this tool when unable to pattern due lack of space
* (Fix) Tool: Layer Repair - When issues are not caculated before, they are computed but user settings are ignored

## 24/07/2020 - v0.6.3.2

* (Add) Tool: Layer Re-Height - Allow change layer height
* (Add) Setting: Gap closing default iterations
* (Add) Setting: Noise removal default iterations
* (Add) Setting: Repair layers and islands by default
* (Add) Setting: Remove empty layers by default
* (Add) Setting: Repair resin traps by default
* (Change) Setting: "Reset to Defaults" changed to "Reset All Settings"
* (Fix) CWS: Lack of ';' on GCode was preventing printer from printing

## 20/07/2020 - v0.6.3.1

* (Add) Preview: Allow import images from disk and replace preview image
* (Add) Setting: Auto zoom to issues and drawings portrait area (best fit)
* (Add) Issue and Pixel Editor ListView can now reorder columns
* (Add) Pixel Editor: Button "Clear" remove all the modifications
* (Add) Pixel Editor: Button "Apply All" to apply the modifications
* (Add) Pixel Editor: Double click items will track and position over the draw
* (Fix) Pixel Editor: Label "Operations" was not reset to 0 after apply the modifications
* (Fix) Pixel Editor: Button "Remove" tooltip
* (Fix) Pixel Editor: Drawing - Bursh Area - px to px�

## 19/07/2020 - v0.6.3.0

* (Add) Layer remove button
* (Add) Tool: Layer removal
* (Add) Layer Repair tool: Remove empty layers
* (Add) Issues: Remove a empty layer will effectively remove the layer
* (Fix) SL1: When converting to other format in some cases the parameters on Printer Notes were not respected nor exported (#12)
* (Fix) Pixel Editor: Draw pixels was painting on wrong positions after apply, when refreshing layer some pixels disappear (Spotted by Nicholas Taylor)

## 17/07/2020 - v0.6.2.3

* (Add) Issue: EmptyLayer - Detects empty layers were image is all black with 0 pixels to cure
* (Add) Toolbar and pushed layer information to bottom
* (Add) Information: Cure pixel count per layer and percentage against total lcd pixels
* (Add) Information: Bounds per layer
* (Add) Zip: Compability with Formware zip files

## 14/07/2020 - v0.6.2.2

* (Add) cbddlp, photon and ctb version 3 compability (Chitubox >= 1.6.5)

## 14/07/2020 - v0.6.2.1

* (Fix) Mutator: Erode was doing pixel dimming

## 14/07/2020 - v0.6.2.0

* (Add) PrusaSlicer Printer: Elegoo Mars 2 Pro
* (Add) PrusaSlicer Printer: Creality LD-002H
* (Add) PrusaSlicer Printer: Voxelab Polaris
* (Add) File Format: UVJ (#8)
* (Add) Mutataor: Pixel Dimming
* (Add) Pixel Editor tab with new drawing functions
* (Add) Pixel Editor: Bursh area and shape
* (Add) Pixel Editor: Supports
* (Add) Pixel Editor: Drain holes
* (Add) Settings for pixel editor
* (Add) Setting: File open default directory
* (Add) Setting: File save default directory
* (Add) Setting: File extract default directory
* (Add) Setting: File convert default directory
* (Add) Setting: File save prompt for overwrite (#10)
* (Add) Setting: File save preffix and suffix name
* (Add) Setting: UVtools version to the title bar
* (Improvement) Force same directory as input file on dialogs
* (Improvement) Pattern: Better positioning when not using an anchor, now it's more center friendly
* (Change) Setting: Start maximized defaults to true
* (Fix) Pattern: Calculated volume was appending one margin width/height more
* (Fix) When cancel a file load, some shortcuts can crash the program as it assume file is loaded
* (Fix) pws: Encode using the same count-of-threshold method as CBDDLP (ezrec/uv3dp#79)

## 02/07/2020 - v0.6.1.1

* (Add) Allow chitubox, phz, pws, pw0 files convert to cws
* (Add) Allow convert between cbddlp, ctb and photon
* (Add) Allow convert between pws and pw0
* (Improvement) Layers can now have modified heights and independent parameters (#9)
* (Improvement) UVtools now generate better gcode and detect the lack of Lift and same z position and optimize the commands
* (Fix) zcodex: Wasn't reporting layer decoding progress

## 02/07/2020 - v0.6.1.0

* (Add) Thumbnail image can now saved to clipboard
* (Add) Setting to allow choose default file extension at load file dialog
* (Add) Double click middle mouse to zoom to fit to image
* (Add) Move mutator to move print volume around the plate
* (Add) Pattern tool
* (Change) Setting window now have tabs to compact the window height
* (Fix) Progress for mutators always show layer count instead of selected range

## 01/07/2020 - v0.6.0.2

* (Add) PrusaSlicer Printer "EPAX X10 4K Mono"
* (Improvement) Better progress window with real progress and cancel button
* (Improvement) Mutators text and name
* (Fix) sl1: After save file gets decoded again
* (Fix) photon, cbddlp, ctb, phz, pws, pw0: Unable to save file, not closed from the decode session
* (Fix) zcodex: Unable to convert file
* (Fix) images: Wasn't opening
* (Fix) images: Wasn't saving
* (Fix) When click on button "New version is available" sometimes it crash the program
* (Fix) Force 1 layer scroll when using Mouse Wheel to scroll the tracker bar
* (Fix) PrusaSlicer printers: Mirror vertically instead to produce equal orientation compared with chitubox

## 29/06/2020 - v0.6.0.1

* (Improvement) Pixel edit now spare a memory cycle per pixel
* (Fix) Resin trap detection was considering layer 0 black pixels as always a drain and skip potential traps on layer 0
* (Fix) Resin trap was crashing when reach -1 layer index and pass the layer count
* (Fix) Pixel edit was crashing the program

## 29/06/2020 - v0.6.0.0

* (Add) UVtools now notify when a new version available is detected
* (Add) Mutator "Flip"
* (Add) Mutator "Rotate"
* (Add) User Settings - Many parameters can now be customized to needs
* (Add) File load elapsed time into Title bar
* (Add) Outline - Print Volume bounds
* (Add) Outline - Layer bounds
* (Add) Outline - Hollow areas
* (Add) Double click layer picture to Zoom To Fit
* (Improvement) Huge performance boost in layer reparing and in every mutator
* (Improvement) Layer preview is now faster
* (Improvement) Islands detection is now better and don't skip any pixel, more islands will show or the region will be bigger
* (Improvement) Islands search are now faster, it will jump from island to island instead of search in every pixel by pixel
* (Improvement) ResinTrap detection and corrected some cases where it can't detect a drain
* (Improvement) Better memory optimization by dispose all objects on operations
* (Improvement) Image engine changed to use only OpenCV Mat instead of two and avoid converting from one to another, as result there's a huge performance gain in some operations (#6)
* (Improvement) UVtools now rely on UVtools.Core, and drop the UVtools.Parser. The Core now perform all operations and transformations inplace of the GUI
* (Improvement) If error occur during save it will show a message with the error
* (Improvement) When rotate layer it will zoom to fit
* (Improvement) Allow zoom to fit to print volume area instead of whole build volume
* (Removed) ImageSharp dependency
* (Removed) UVtools.Parser project
* (Fix) Nova3D Elfin printer values changed to Display Width : 131mm / Height : 73mm & Screen X: 2531 / Y: 1410 (#5)
* (Fix) Fade resizes make image offset a pixel from layer to layer because of integer placement, now it matain the correct position
* (Fix) sl1: AbsoluteCorrection, GammaCorrection, MinExposureTime, MaxExposureTime, FastTiltTime, SlowTiltTime and AreaFill was byte and float values prevents the file from open (#4)
* (Fix) zcodex: XCorrection and YCorrection was byte and float values prevents the file from open (#4)
* (Fix) cws: XCorrection and YCorrection was byte and float values prevents the file from open (#4)
* (Fix) cws: Wrong # char on .gcode file prevent from printing (#4)

## 21/06/2020 - v0.5.2.2

* (Fix) phz: Files with encryption or sliced by chitubox produced black images after save due not setting the image address nor size (Spotted by Burak Cezairli)

## 20/06/2020 - v0.5.2.1

* (Add) cws: Allow change layer PWM value
* (Update) Dependency ImageSharp from 1.0.0-rc0002 to 1.0.0-rc0003 (It fix a error on resize function)
* (Fix) cws: GCode 0 before G29
* (Fix) Phrozen Sonic Mini: Display Height from 66.04 to 68.04
* (Fix) Zortrax Inkspire: Display and Volume to 74.67x132.88
* (Fix) Layer repair tool allow operation when every repair checkbox is deselected

## 19/06/2020 - v0.5.2

* (Add) Resin Trap issue validator and repairer - Experimental Feature (#3)
* (Add) Layer Repair tool can now fix Resin Traps when selected
* (Add) "Remove" issues button fix selected Resin traps, the operation now run under a thread and in a parallel way, preventing the GUI from freeze
* (Change) "Repair Layers" button renamed to "Repair Layers and Issues"
* (Fix) When do a "repair layers" before open the Issue tab, when open next it will recompute issues without the need

## 18/06/2020 - v0.5.1.3

* (Add) Button save layer image to Clipboard 
* (Change) Go to issue now zoom at bounding area instead of first pixels
* (Change) Layer navigation panel width increased in 20 pixels, in some cases it was overlaping the slider
* (Change) Actual layer information now have a depth border
* (Change) Increased main GUI size to X: 1800 and Y: 850 pixels
* (Change) If the GUI window is bigger than current screen resolution, it will start maximized istead
* (Fix) cbddlp: AntiAlias is number of _greys_, not number of significant bits (ezrec/uv3dp#75)
* (Fix) Outline not working as before, due a forget to remove test code

## 17/06/2020 - v0.5.1.2

* (Add) Able to install only the desired profiles and not the whole lot (Suggested by: Ingo Strohmenger)
* (Add) Update manager for PrusaSlicer profiles
* (Add) If PrusaSlicer not installed on system it prompt for installation (By open the official website)
* (Fix) Prevent profiles instalation when PrusaSlicer is not installed on system
* (Fix) The "Issues" computation sometimes fails triggering an error due the use of non concurrent dictionary
* (Fix) Print profiles won't install into PrusaSlicer

## 16/06/2020 - v0.5.1.1

* (Add) photon, cbddlp, ctb and phz can be converted to Zip
* (Fix) ctb: When AntiAliasing is on it saves a bad file

## 16/06/2020 - v0.5.1

* (Add) Zip file format compatible with chitubox zip
* (Add) PrusaSlicer Printer "Kelant S400"
* (Add) PrusaSlicer Printer "Wanhao D7"
* (Add) PrusaSlicer Printer "Wanhao D8"
* (Add) PrusaSlicer Printer "Creality LD-002R"
* (Add) Shortcut "CTRL+C" under Issues listview to copy all selected item text to clipboard
* (Add) Shortcut "ESC" under Properties listview to deselect all items
* (Add) Shortcut "CTRL+A" under Properties listview to select all items
* (Add) Shortcut "*" under Properties listview to invert selection
* (Add) Shortcut "CTRL+C" under Properties listview to copy all selected item text to clipboard
* (Add) Resize function can now fade towards 100% (Chamfers)
* (Add) Solidify mutator, solidifies the selected layers, closes all inner holes
* (Change) Renamed the project: UVtools
* (Change) On title bar show loaded filename first and program version after
* (Improvement) Increased Pixel column width on Issues tab listview
* (Fix) Resize function can't make use of decimal numbers
* (Fix) CWS gcode was setting M106 SO instead of M106 S0
* (Fix) CWS disable motors before raise Z after finish print

## 13/06/2020 - v0.5

* (Add) PWS and PW0 file formats (Thanks to Jason McMullan)
* (Add) PrusaSlicer Printer "AnyCubic Photon S"
* (Add) PrusaSlicer Printer "AnyCubic Photon Zero"
* (Add) PrusaSlicer Universal Profiles optimized for non SL1 printers (Import them)
* (Add) Open image files as single layer and transform them in grayscale (jpg, jpeg, png, bmp, gif, tga)
* (Add) Resize mutator
* (Add) Shortcut "F5" to reload current layer preview
* (Add) Shortcut "Home" and button go to first layer
* (Add) Shortcut "End" and button go to last layer
* (Add) Shortcut "+" and button go to next layer
* (Add) Shortcut "-" and button go to previous layer
* (Add) Shortcut "CTRL+Left" go to previous issue if available
* (Add) Shortcut "CTRL+Right" go to next issue if available
* (Add) Shortcut "Delete" to remove selected issues
* (Add) Button to jump to a layer number
* (Add) Show current layer and height near tracker position
* (Add) Auto compute issues when click "Issues" tab for the first time for the open file
* (Add) "AntiAliasing_x" note under PrusaSlicer printer to enable AntiAliasing on supported formats, printers lacking this note are not supported
* (Add) AntiAliasing capable convertions
* (Add) Touching Bounds detection under issues
* (Change) Scroll bar to track bar
* (Change) Keyword "LiftingSpeed" to "LiftSpeed" under PrusaSlicer notes (Please update printers notes or import them again)
* (Change) Keywords For Nova3D Elfin printer under PrusaSlicer notes (Please update printers notes or import them again)
* (Change) Keywords For Zortrax Inkspire printer under PrusaSlicer notes (Please update printers notes or import them again)
* (Change) Islands tab to Issues ab
* (Improvement) Much faster photon, cbddlp, cbt and phz file encoding/convert and saves
* (Improvement) Much faster layer scroll display
* (Improvement) Hide empty items for status bar, ie: if printer don't have them to display
* (Improvement) Smooth mutators descriptions
* (Improvement) Disallow invalid iteration numbers for smooth mutators
* (Improvement) File reload now reshow current layer after reload
* (Improvement) Some dependecies were updated and ZedGraph removed
* (Fix) AntiAlias decodes for photon and cbddlp
* (Fix) AntiAlias encodes and decodes for cbt
* (Fix) Save the preview thumbnail image trigger an error
* (Fix) Implement missing "InheritsCummulative" key to SL1 files
* (Fix) Install print profiles button, two typos and Cancel button doesn't really cancel the operation

## 05/06/2020 - v0.4.2.2 - Beta

* (Add) Shortcut "ESC" under Islands listview to deselect all items
* (Add) Shortcut "CTRL+A" under Islands listview to select all items
* (Add) Shortcut "*" under Islands listview to invert selection
* (Add) Shortcut "CTRL+F" to go to a layer number
* (Change) Layer image is now a RGB image for better manipulation and draws
* (Change) Layer difference now shows previous and next layers (only pixels not present on current layer) were previous are pink and next are cyan, if a pixel are present in both layers a red pixel will be painted.
* (Fix) Save modified layers on .cbddlp and .cbt corrupts the file to print when Anti-Aliasing is used (> 1)
* (Fix) cbdlp layer encoding

## 04/06/2020 - v0.4.2.1 - Beta

* (Add) PrusaSlicer Printer "AnyCubic Photon"
* (Add) PrusaSlicer Printer "Elegoo Mars Saturn"
* (Add) PrusaSlicer Printer "Elegoo Mars"
* (Add) PrusaSlicer Printer "EPAX X10"
* (Add) PrusaSlicer Printer "EPAX X133 4K Mono"
* (Add) PrusaSlicer Printer "EPAX X156 4K Color"
* (Add) PrusaSlicer Printer "Peopoly Phenom L"
* (Add) PrusaSlicer Printer "Peopoly Phenom Noir"
* (Add) PrusaSlicer Printer "Peopoly Phenom"
* (Add) PrusaSlicer Printer "Phrozen Shuffle 4K"
* (Add) PrusaSlicer Printer "Phrozen Shuffle Lite"
* (Add) PrusaSlicer Printer "Phrozen Shuffle XL"
* (Add) PrusaSlicer Printer "Phrozen Shuffle"
* (Add) PrusaSlicer Printer "Phrozen Sonic"
* (Add) PrusaSlicer Printer "Phrozen Transform"
* (Add) PrusaSlicer Printer "QIDI Shadow5.5"
* (Add) PrusaSlicer Printer "QIDI Shadow6.0 Pro"
* (Add) "Detect" text to compute layers button
* (Add) "Repair" islands button on Islands tab
* (Add) "Highlight islands" button on layer toolbar
* (Add) Possible error cath on island computation
* (Add) After load new file layer is rotated or not based on it width, landscape will not rotate while portrait will
* (Improvement) Highlighted islands now also show AA pixels as a darker yellow
* (Improvement) Island detection now need a certain number of touching pixels to consider a island or not, ie: it can't lay on only one pixel
* (Fix) Island detection now don't consider dark fadded AA pixels as safe land
* (Fix) Epax X1 printer properties

## 03/06/2020 - v0.4.2 - Beta

* (Add) Zoom times information
* (Add) Island checker, navigation and removal
* (Add) Layer repair with island repair
* (Add) Show mouse coordinates over layer image
* (Fix) Pixel edit cant remove faded AA pixels
* (Fix) Pixel edit cant add white pixels over faded AA pixels
* (Change) Nova3D Elfin printer build volume from 130x70 to 132x74

## 01/06/2020 - v0.4.1 - Beta

* (Add) Opening, Closing and Gradient Mutators
* (Add) Choose layer range when appling a mutator #1
* (Add) Choose iterations range/fading when appling a mutator (Thanks to Renos Makrosellis)
* (Add) Global and unhandled exceptions are now logged to be easier to report a bug
* (Change) Current layer and layer count text was reduced by 1 to match indexes on mutators
* (Improvement) Better mutator dialogs and explanation
* (Improvement) Compressed GUI images size
* (Fix) SlicerHeader was with wrong data size and affecting .photon, .cbddlp and .cbt (Thanks to Renos Makrosellis)


## 27/05/2020 - v0.4 - Beta

* (Add) CWS file format
* (Add) Nova3D Elfin printer
* (Add) Zoom and pan functions to layer image
* (Add) Pixel editor to add or remove pixels
* (Add) Outline layer showing only borders
* (Add) Image mutators, Erode, Dilate, PyrDownUp, Smooth
* (Add) Task to save operation
* (Add) Printers can be installed from GUI Menu -> About -> Install printers into PrusaSlicer
* (Improvement) Layer Management
* (Improvement) Faster Save and Save As operation
* (Fix) Bad layer image when converting SL1 to PHZ
* (Fix) Corrected EncryptionMode for PHZ files
* (Fix) Save As can change file extension
* (Fix) Save As no longer reload file
* (Fix) SL1 files not accepting float numbers for exposures
* (Fix) SL1 files was calculating the wrong layer count when using slow layer settings
* (Fix) Modifiers can't accept float values
* (Fix) Sonic Mini prints mirroed
* (Fix) Layer resolution shows wrong values

## 21/05/2020 - v0.3.3.1 - Beta

* (Fix) Unable to convert Chitubox or PHZ files when enconter repeated layer images

## 19/05/2020 - v0.3.3 - Beta

* (Add) PHZ file format
* (Add) "Phrozen Sonic Mini" printer
* (Add) Convert Chitubox files to PHZ files and otherwise
* (Add) Convert Chitubox and PHZ files to ZCodex
* (Add) Elapsed seconds to convertion and extract dialog
* (Improvement) "Convert To" menu now only show available formats to convert to, if none menu is disabled
* (Fix) Enforce cbt encryption
* (Fix) Not implemented convertions stay processing forever


## 11/05/2020 - v0.3.2 - Beta

* (Add) Show layer differences where daker pixels were also present on previous layer and the white pixels the difference between previous and current layer.
* (Add) Layer preview process time in milliseconds
* (Add) Long operations no longer freeze the GUI and a progress message will shown on those cases
* (Improvement) Cache layers were possible for faster operation
* (Improvement) As layer data is now cached, input file is closed after read, this way file wouldn't be locked for other programs
* (Improvement) Speed up extraction with parallelism
* (Improvement) Extract output folder dialog now open by default on from same folder as input file
* (Improvement) Extract now create a folder with same file name to dump the content
* (Fix) Extract to folder was wiping the target folder, this is now disabled to prevent acidental data lost, target files will be overwritten

## 30/04/2020 - v0.3.1 - Beta

* (Add) Thumbnails to converted photon and cbddlp files
* (Add) ctb file format
* (Add) Show possible extensions/files under "Convert To" menu
* (Add) Open new file in a new window without lose current work
* (Improvement) Rename and complete some Chitubox properties
* (Improvement) More completion of cbddlp file
* (Improvement) Optimized layer read from cbddlp file
* (Improvement) Add layer hash code to encoded Chitubox layers in order to optimize file size in case of repeated layer images
* (Improvement) GUI thumbnail preview now auto scale splitter height to a max of 400px when change thumbnail
* (Improvement) After convertion program prompt for open the result file in a new window
* (Change) Move layer rotate from view menu to layer menu
* (Change) Cbbdlp convertion name to Chitubox
* (Change) On convert, thumbnails are now resized to match exactly the target thumbnail size
* (Change) GUI will now show thumbnails from smaller to larger
* (Fix) RetractFeedrate was incorrectly used instead of LiftFeedrate on Zcodex gcode

## 27/04/2020 - v0.3.0 - Beta

* (Add) zcodex file format
* (Add) Zortrax Inkspire Printer
* (Add) Properties menu -- Shows total keys and allow save information to a file
* (Add) "GCode" viewer Tab -- Only for formats that incldue gcode into file (ie: zcodex)
* (Add) Save gcode to a text file
* (Add) Allow to vertical arrange height between thumbnails and properties
* (Improvement) Thumbnail section is now hidden if no thumbnails avaliable
* (Improvement) Thumbnail section now vertical auto scales to the image height on file load
* (Improvement) On "modify properties" window, ENTER key can now be used to accept and submit the form
* (Fixed) Current model height doesn't calculate when viewing cbddlp files
* (Change) Round values up to two decimals
* (Change) Move actual model height near total height, now it shows (actual/total mm)
* (Change) Increase font size
* (Change) Rearrange code

## 22/04/2020 - v0.2.2 - Beta

* (Add) File -> Reload
* (Add) File -> Save
* (Add) File -> Save As
* (Add) Can now ajust some print properties
* (Add) 'Initial Layer Count' to status bar
* (Add) Allow cbbdlp format to extract 'File -> Extract'
* (Add) Thumbnail resolution label
* (Add) Layer resolution label
* (Add) Allow save current layer image
* (Change) Rearrange menu edit items to file
* (Change) Edit some shortcuts
* (Change) Strict use dot (.) for real numbers instead of comma (,)

## 15/04/2020 - v0.2.1 - Beta

* (Add) Allow open other file formats as well on viewer
* (Add) All thumbnails can now be seen and saved
* (Add) Rotate layer image
* (Add) Close file
* (Change) more abstraction
* (Change) from PNG to BMP compression to speed up bitmap coversion
* (Change) Faster layer preview

## 12/04/2020 - v0.2 - Beta

* (Add) cbddlp file format
* (Add) "convert to" function, allow convert sl1 file to another
* (Add) EPAX X1 printer
* (Change) Code with abstraction of file formats

## 06/04/2020 - V0.1 - Beta

* First release for testing