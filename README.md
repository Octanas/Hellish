## Development Environment Configuration

To get this project working on Unity, you have to install the FMOD integration package. It is not included in the source code, but components that require it are. To install the FMOD integration package, do the following:

1. Clone (or pull) the project
2. Get the FMOD integration package from the Asset Store
3. Import the package to the project using Unity's Package Manager
4. In the FMOD's integration package manager, select `Single Platform Build` and set the build path to `/FMODAssets/Desktop/`
5. Reset the project: `git reset --hard` (this step is done because what was added is not mean to be pushed to the repo, it will be used only locally)
6. It should be working now