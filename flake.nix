{
  description = "Blazor Server project with PostgreSQL";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
      in {
        devShells.default = pkgs.mkShell {
          buildInputs = [
            pkgs.postgresql
          ];

          shellHook = ''
            export PGDATA=$PWD/.pgdata
            if [ ! -d "$PGDATA" ]; then
              initdb --auth=md5 --username=postgres --pwfile=<(echo 1234)
            fi
            pg_ctl -D "$PGDATA" -l "$PGDATA/logfile" start

            echo "PostgreSQL запущен. Пользователь: postgres, пароль: 1234"
            echo "Для подключения: psql -U postgres"
          '';
        };
      });
}