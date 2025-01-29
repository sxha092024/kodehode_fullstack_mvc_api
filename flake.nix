{
  description = "Project generated from dotnet flake template";

  inputs = {
    flake-utils.url = "github:numtide/flake-utils";
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  };

  outputs = {
    self,
    nixpkgs,
    flake-utils,
  }:
    flake-utils.lib.eachDefaultSystem (
      system: let
        pkgs = import nixpkgs {
          inherit system;
          # config.allowUnfree = true;
        };

        dotnetPkg = with pkgs.dotnetCorePackages;
          combinePackages [
            sdk_9_0
            aspnetcore_9_0
          ];
      in {
        formatter = pkgs.alejandra;

        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [];

          nativeBuildInputs = with pkgs; [
            dotnetPkg
            csharp-ls

            omnisharp-roslyn

            csharpier

            # db and tooling
            sqlite
            sqlitebrowser
          ];
          shellHook = ''
            export DOTNET_ROOT="${dotnetPkg}";
            echo ".net root: '${dotnetPkg}'"
            echo ".net version: $(${dotnetPkg}/bin/dotnet --version)"
            echo ".net SDKs:"
            echo "$(${dotnetPkg}/bin/dotnet --list-sdks)"
          '';
        };
      }
    );
}
