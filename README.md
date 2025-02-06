# Opilo HazardZone Monitor

**Opilo HazardZone Monitor** is a safety application designed to monitor and alert when individuals are present in restricted or hazardous areas. This project serves dual purposes:
- **Primary Goal:** Enhance safety by providing real-time monitoring and alerts for unauthorized or dangerous zone entries.
- **Learning Objective:** To explore and learn modern web technologies including Blazor, React, and Microsoft Aspire.

## Features (Planned)

- **Real-time Location Tracking:** Monitor the presence of individuals in specific zones.
- **Zone Management:** Define, edit, and manage hazard zones through an intuitive interface.
- **Alert System:** Notify relevant parties when breaches occur.
- **User Interface:** A responsive and user-friendly dashboard for monitoring and administration.

## Technologies

- **Blazor:** For creating interactive web UIs using C# instead of JavaScript.
- **React:** To build the front-end components enhancing user experience and interaction.
- **Microsoft Aspire:** To orchestrate and manage the cloud-native applications, currently in development stages for this project.

## Current Status

The project is in its early stages:

- **Domain Model:** Under active development. This includes defining entities, relationships, and business logic for hazard zones and user tracking.

## Getting Started

### Prerequisites

- .NET SDK (version to be specified)
- Node.js (for React parts)
- A modern web browser

### Installation

1. **Clone the repository:**
   ```sh
   git clone https://github.com/yourusername/Opilo-HazardZone-Monitor.git
   ```
2. **Navigate to the project folder:**
   ```sh
   cd Opilo-HazardZone-Monitor
   ```
3. **Install dependencies:**
    - For .Net Components:
      ```sh
      dotnet restore
      ```
      - For React components:
      ```sh
      npm install
      ```
4. **Run the application:**
   - Start the Blazor server:
     ```sh
     dotnet run --project PathToYourBlazorProject
     ```
   - If there's a React part, navigate to its directory and:
     ```sh
     npm start
     ```

### Development

- **Frontend:** Focus on creating the UI with Blazor and React.
- **Backend:** Implement the domain model, database interactions, and business logic.

## Roadmap

- **Q1 2025:** Complete domain model and basic UI.
- **Q2 2025:** Implement real-time monitoring features.
- **Q3 2025:** Enhance UI, add alert systems, and begin integration with Aspire.
- **Q4 2025:** Full system integration, testing, and documentation.

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the GNU Affero General Public License. See `LICENSE` for more information.

## Contact

Niels Heirbaut - [hazardzone@opilo.nl](mailto:hazardzone@opilo.nl)

Project Link: https://github.com/nheirbaut/Opilo.HazardZoneMonitor

