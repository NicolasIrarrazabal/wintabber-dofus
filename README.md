# 🪟 Wintabber Dofus - Gestor de Ventanas

**Herramienta profesional para organizar y gestionar múltiples ventanas de Dofus Retro**

![Wintabber Dofus](assets/banner.png)

## 🎯 Características Principales

### **Organización de Ventanas**
- **Captura automática** de ventanas de Dofus Retro
- **Incrustado en pestañas** - Todas las ventanas en una sola interfaz
- **Drag & Drop** - Reorganiza pestañas arrastrando
- **Redimensionamiento automático** - Las ventanas se adaptan al tamaño

### 📋 **Sistema de Layouts Completo**
- **💾 Guardar Layouts** - Presiona F4 para guardar posición actual
- **🔄 Cargar Layouts** - Presiona F5 para cargar un layout guardado
- **📋 Gestor de Layouts** - Presiona F6 para administrar todos los layouts
- **Acceso Rápido** - Ctrl+Alt+1..9 para cargar layouts directamente

### ⌨️ **Atajos de Teclado**
| Combinación | Función |
|-------------|----------|
| **F1** | Ventana anterior |
| **F2** | Ventana siguiente |
| **F3** | Mostrar/Ocultar menú |
| **F4** | Guardar layout actual |
| **F5** | Cargar último layout |
| **F6** | Abrir gestor de layouts |
| **Ctrl+Alt+1..9** | Cargar layout directamente |

## 📁 **Ubicación de Layouts**

Los layouts se guardan automáticamente en:
```
%APPDATA%\DofusMiniTabber\window_positions.json
```

### **Ruta Completa en Windows:**
```
C:\Users\{TU_USUARIO}\AppData\Roaming\DofusMiniTabber\window_positions.json
```

### **📋 Formato del Archivo JSON:**
```json
{
  "Nombre del Layout": {
    "Name": "Nombre del Layout",
    "Positions": [
      {
        "WindowName": "Personaje1",
        "Position": 0
      },
      {
        "WindowName": "Personaje2", 
        "Position": 1
      }
    ],
    "CreatedAt": "2026-03-28T00:30:00.000Z",
    "Description": "Descripción del layout"
  }
}
```

## 🚀 **Instalación y Uso**

### 📦 **Instalación:**
1. **Descarga** el ejecutable desde [Releases](../../releases)
2. **Ejecuta** `WintabberDofus.exe`
3. **¡Listo!** No requiere instalación

### 🎮 **Uso Básico:**
1. **Abre Dofus Retro** con múltiples cuentas
2. **Ejecuta Wintabber Dofus**
3. **Presiona "⚡ CAPTURAR VENTANAS"**
4. **Tus ventanas se organizarán** en pestañas

### 💾 **Gestión de Layouts:**
1. **Organiza tus ventanas** como prefieras
2. **Presiona F4** - Guarda layout con nombre
3. **Presiona F6** - Abre gestor completo
4. **Usa Ctrl+Alt+1..9** - Acceso rápido a favoritos

## 🔧 **Requisitos del Sistema**

### **Mínimos:**
- **Windows 10/11** (64-bit)
- **.NET Framework 4.7.2** o superior
- **2 GB RAM** mínimos
- **50 MB** espacio en disco

### **Recomendados:**
- **Windows 11** 
- **.NET 6.0+**
- **4 GB RAM**
- **100 MB** espacio en disco

## 🎯 **Características Técnicas**

### 🛠️ **Tecnología:**
- **C# .NET 6.0** - Alto rendimiento
- **Win32 API** - Integración nativa con Windows
- **JSON System.Text** - Gestión eficiente de configuraciones
- **Windows Forms** - Interfaz nativa y rápida

### 🔒 **Seguridad:**
- **Sin conexión a internet** - 100% offline
- **Sin telemetría** - No recopila datos
- **Código abierto** - Transparente y verificable
- **Sin instalación** - Ejecutable portable

## 🎨 **Interfaz de Usuario**

### 🎨 **Diseño Moderno:**
- **Tema oscuro** profesional
- **Barra flotante** semitransparente
- **Iconos intuitivos** con emojis
- **Animaciones suaves** de transición

### 📱 **Controles Intuitivos:**
- **Drag & Drop** de pestañas
- **Click derecho** menú contextual
- **Hotkeys globales** funcionan en cualquier aplicación
- **Auto-ocultar** menú cuando no se necesita

## 🔧 **Configuración Avanzada**

### ⚙️ **Personalización:**
- **Posición de barra flotante**
- **Opacidad y transparencia**
- **Atajos personalizados**
- **Auto-inicio con Windows**

### 📂 **Gestión de Archivos:**
- **Exportar layouts** - Comparte configuraciones
- **Importar layouts** - Migra desde otro PC
- **Backup automático** - Copias de seguridad
- **Sincronización** con Dofus Tools

## 🐛 **Solución de Problemas**

### ❓ **Preguntas Frecuentes:**

**Q: No captura las ventanas de Dofus**
- Asegúrate de que Dofus esté en modo ventana
- Ejecuta Wintabber como administrador
- Verifica que .NET Framework esté instalado

**Q: Los hotkeys no funcionan**
- Revisa que no estén en uso por otro programa
- Reinicia la aplicación
- Verifica configuración de teclado

**Q: Se pierden los layouts**
- Los layouts guardan en `%APPDATA%\DofusMiniTabber\`
- Haz backup de `window_positions.json`
- Verifica permisos de escritura

### 🐛 **Reporte de Issues:**
Si encuentras un error, por favor reporta en [Issues](../../issues) con:
- **Descripción detallada** del problema
- **Pasos para reproducir**
- **Capturas de pantalla**
- **Versión de Windows y .NET**

## 🤝 **Compatibilidad**

### ✅ **Compatible con:**
- **Dofus Retro** - Todas las versiones
- **Dofus Tools Updated 2.0** - Comparte layouts
- **Windows 10/11** - Todas las ediciones
- **Múltiples monitores** - Soporte completo

### 🔄 **Integración:**
- **Comparte layouts** con Dofus Tools
- **Misma ubicación** de archivo JSON
- **Sincronización automática** entre programas
- **Importación/Exportación** de configuraciones

## 📸 **Capturas de Pantalla**

### 🎮 **Interfaz Principal:**
*(Agrega capturas aquí)*

### 📋 **Gestor de Layouts:**
*(Agrega capturas aquí)*

### ⌨️ **Hotkeys en Acción:**
*(Agrega capturas aquí)*

## 🏗️ **Para Desarrolladores**

### 🛠️ **Compilar desde Código:**
```bash
git clone https://github.com/tu-usuario/wintabber-dofus.git
cd wintabber-dofus
dotnet build --configuration Release
```

### 📁 **Estructura del Proyecto:**
```
src/
├── Form1.cs                    # Ventana principal
├── LayoutSelectorForm.cs        # Gestor de layouts
├── WindowPositionManager.cs     # Sistema de guardado
├── Program.cs                  # Punto de entrada
└── DofusMiniTabber.csproj     # Configuración
```

## 📄 **Licencia**

Este proyecto está bajo la **Licencia MIT** - ver archivo [LICENSE](LICENSE) para detalles.

### 🎯 **Permiso:**
- ✅ Uso comercial
- ✅ Modificación
- ✅ Distribución
- ✅ Uso privado
- ❌ Responsabilidad limitada

## 🙏 **Agradecimientos**

- **Comunidad Dofus Retro** - Feedback y testing
- **Microsoft .NET Team** - Excelente framework
- **Contribuidores** - Mejoras y sugerencias
- **Beta testers** - Reporte de bugs

## 📞 **Contacto y Soporte**

- **🐛 Issues**: [Reportar problemas](../../issues)
- **💡 Sugerencias**: [Request features](../../issues/new?template=feature_request)
- **📧 Email**: *(agrega tu email si quieres)*
- **💬 Discord**: *(agrega tu servidor si tienes)*

---

## ⭐ **¿Te gusta el proyecto?**

**¡No olvides darle una estrella!** ⭐

**Ayuda a más jugadores a organizar sus ventanas de Dofus** 🎮

---

**🔥 Descarga la última versión en [Releases](../../releases) 🔥**

**📚 Documentación completa en [Wiki](../../wiki) 📚**

**💬 Únete a la comunidad en [Discussions](../../discussions) 💬**
