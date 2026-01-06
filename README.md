# Inziv Home Assignment

This project is a Hardware Monitor application. Follow the instructions below to get started.

---

## 1. Clone the project from GitHub
git clone https://github.com/Miriam-Armon/Inziv.git
cd Inziv
## 2. Configure the application

Before running the application, set the path to the hardware file in the appsettings.json file.

Open appsettings.json and edit the HardwareMonitorOptions section:

{
  "HardwareMonitorOptions": {
    "HardwareFilePath": ""
  }
}


Replace "" with the correct path to your hardware file.

Use double backslashes \\ for Windows paths.
---
## 3. Run the application
```bash
