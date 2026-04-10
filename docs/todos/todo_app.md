The App.tsx is way too bloated. It uses a lot of hooks which could be separate files, it kind of declares components, which arent real components (like Main Area), that could be put into their own component file. Then we could have a component/folder structure, where the component folder contains folders for each high level component which in turn contains the tsx file and a folder for all the subcomponents that are in that component. ofcourse shared components need their own folder. Like how is there not a component / file for Card, does that make sense?

Also we can define some "wrapper components" that dont necessarily have to be visible on screen but which makes it easier to plan the layout and the single components less convoluted.

Please try and execute this to the best of you ability.

Also some constants in some components could maybe be extracted into separate files (maybe as JSON or what could be an appropriate file type? is there something better for typescript?) SO that theyre clearly separated from the code