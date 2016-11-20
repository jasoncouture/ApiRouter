# ApiRouter

This project is a simple load balancer that's integrated with Consul. It's configurable from Consul's key/value store using JSON loaded in specific paths.

## Installation

Build with visual studio 2015 or MSBuild, copy output folder and run it. By default it should start successfully, listening on port 8080. Any requests to this port without a configuration should result in an internal server error.

## Usage

TODO

## Documentation

TODO

## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

## History

11/20/2016 - Initial public release.

## Credits

Jason Couture - jcouture@pssproducts.com

## [License](LICENSE.md)
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
