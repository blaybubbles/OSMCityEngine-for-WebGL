# OSM City Engine for WebGL

This project is an adaptation and enhancement of the Polimi OSM City Engine, originally developed by [BerkeCagkanToptas](https://github.com/BerkeCagkanToptas/OSM-City-Engine). In this version, we have made several improvements, restored missing files, and optimized it for WebGL rendering.

## Overview

The OSM City Engine is a powerful tool for generating 3D city models based on OpenStreetMap (OSM) data. It allows you to transform OSM data into detailed 3D representations of urban environments. This adaptation focuses on enhancing the project's capabilities and making it more accessible for web-based visualization through WebGL technology.
## Motivation

This project was born out of a personal interest in bringing an existing project to life and exploring its potential in a web-based environment. The primary motivations for this project were:

- **Web Adaptation:** The goal was to determine whether it was possible to adapt an existing Unity 3D project into a web application with minimal modifications.

- **Mobile Compatibility:** We aimed to investigate the feasibility of running the project in a web browser on mobile devices, providing a broader audience with access to the 3D city model.

- **Graphics Exploration:** We were curious about how the Universal Render Pipeline (URP) would perform in real-time and wanted to visually enhance the project.

- **Optimization Challenges:** A significant portion of the project involved transitioning from synchronous to asynchronous operations and integrating the Overpass API to enhance performance and responsiveness.

- **Exploring New Tools:** As a hobbyist project, we took the opportunity to experiment with the new Input System and UI Toolkit in Unity, learning how to implement these tools effectively.

- **Camera Modes:** We introduced the option to switch between free fly and third-person camera modes, enhancing user exploration and interaction.

- **UI Toolkit and Event Handling:** The UI Toolkit's flexibility was appreciated, although manually handling events proved to be more time-consuming but ultimately rewarding.

Overall, this project served as a learning experience and an opportunity to apply new techniques and technologies to an existing project.


## Features

- **WebGL Rendering:** We've adapted the OSM City Engine to utilize WebGL for rendering, making it suitable for web-based applications and interactive 3D visualization on modern browsers.

- **Bug Fixes:** We've addressed known issues and fixed bugs in the original project to ensure smoother performance and a more reliable user experience.

- **File Recovery:** Some essential files were missing in the original project. We've located and restored these files to ensure the project's functionality.

## Getting Started

To get started with this adapted version of the OSM City Engine, follow these steps:

1. **Clone the Repository:** Clone this repository to your local machine using the following command:

   ```shell
   git clone https://github.com/your-username/OSM-City-Engine-WebGL.git
   ```

2. **Setup Environment:** Make sure you have the necessary dependencies installed to run the project. Refer to the documentation for details on setting up the environment.

3. **Build and Run:** Build the project and start the server. You can then access the WebGL-based OSM City Engine in your browser.

## Usage

### Accessing the Application

You can access the OSM City Engine WebGL application by visiting [https://osm.blaybubbles.com/](https://osm.blaybubbles.com/) in your web browser.

### Creating a City Model

To create a 3D city model using our application, follow these steps:

    1. **Enter City Name or Coordinates:** In the city model creation panel, provide either the city's name or its coordinates. You can copy coordinates from any map source. If you enter a city name and press enter, the application show the coordinates for that city below input field in the format "latitude, longitude".
   [City Model Panel](https://github.com/blaybubbles/OSMCityEngine-for-WebGL/blob/main/images/load_menu.png)
   _City model creation panel._

2. **Tile Source and Elevation Map:** Currently, the options for tile source and elevation map are not fully developed. For now, the application uses Bing Maps for tile images and NASE Strm G3 for elevation data.

3. **Adjust City Size:** Use the slider "Slider" to specify the dimensions of your city. The default size is set to 1 kilometer, with a maximum recommended size of 2 kilometers for web-based rendering.

4. **Render the City:** Once you've configured the city parameters, click the "Render" button to generate the 3D city model.

![Example of a City Model](https://github.com/blaybubbles/OSMCityEngine-for-WebGL/blob/main/images/7_.png)
_Example of a city model generated using the OSM City Engine._

### Interacting with the City Model

In the OSM City Engine WebGL application, you can interact with the city model in two camera modes:

1. **Free Fly Mode:** In this mode, you have complete freedom to navigate the 3D city model. You can fly around, zoom, and rotate the view to explore the city from any angle.
![Free Fly Mode](https://github.com/blaybubbles/OSMCityEngine-for-WebGL/blob/main/images/4_.png)

2. **Third Person Mode:** To switch to third-person mode, click the button with a person icon located in the upper right corner of the screen. In this mode, the camera is positioned behind a virtual character, providing a different perspective of the city.

   To return to free fly mode, simply press the "O" key.

   ![Third Person Button](https://github.com/blaybubbles/OSMCityEngine-for-WebGL/blob/main/images/5_.png)

## Contribution

I welcome contributions from the community to help improve and expand the project. If you encounter issues or have suggestions, please open an issue or submit a pull request. I will review all contributions and merge them into the project as appropriate.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.

## Acknowledgments

I would like to express our gratitude to [BerkeCagkanToptas](https://github.com/BerkeCagkanToptas) for their original work on the Polimi OSM City Engine, which served as the foundation for this adaptation.
