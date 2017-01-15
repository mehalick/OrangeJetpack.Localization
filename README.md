# OrangeJetpack.Localization

[![Build status](https://ci.appveyor.com/api/projects/status/hoqf1taijirw7h84/branch/master?svg=true)](https://ci.appveyor.com/project/AndyMehalick/orangejetpack-localization/branch/master) [![NuGet](https://img.shields.io/nuget/v/OrangeJetpack.Localization.svg?maxAge=3600)](https://www.nuget.org/packages/OrangeJetpack.Localization/)

*UPDATE: Version 2.0.0 has been ported to .NET Core (.NET Standard 1.6).*

Orange Jetpack Localization is a library that simplifies the storage and retrieval of multi-language POCOs in a database. It provides interfaces and attributes for setting up localizable classes and extension methods for setting and getting objects and properties.

Original blog post:

https://andy.mehalick.com/2013/09/07/localizing-entity-framework-poco-properties-with-json-part-1/

## Getting Started

```powershell
Install-Package OrangeJetpack.Localization
```

### Creating Localizable Classes

Localization provides an interface and a property attribute for setting up classes that can be localized:

```csharp
public class Planet : ILocalizable
{
    [Localized]
    public string Name { get; set; }
}
```

### Settings Properties from Dictionaries

You can use the Set<T>() extension method to set properties directly:

```csharp
var planet = new Planet();

var name = new Dictionary<string, string>
{
    {"en", "Earth"},
    {"ru", "Земля"},
    {"ja", "地球"},
    {"ar", "أرض" }
};

planet.Set(i => i.Name, name);
```

### Setting Properties from Model Binding:

```csharp
[HttpPost, ValidateAntiForgeryToken]
public ActionResult Edit(Planet planet, LocalizedContent[] name)
{
    planet.Set(i => i.Name, name);
    
    //...

    return View();
}
```

### Localizing an Object

You can use Localize<T>() extension methods to get a localized instance of an ILocalizable class or collection.

#### Single Object

```csharp
var planet = _db.Planets.SingleOrDefault(i => i.Id == 1).Localize("en");
```

#### Localizing a Collection

```csharp
var planets = _db.Planets.Localize<Planet>("en");
```

#### Localizing Specific Properties

```csharp
var planets = _db.Planets.Localize("en", i => i.Name);
```

#### Controlling Depth of Localization

```csharp
// localizes only root objects (DEFAULT)
var planets = _db.Planets.Localize<Planet>("en", LocalizationDepth.Shallow);

// localizes only root objects and immediate children (properties and collections)
var planets = _db.Planets.Localize<Planet>("en", LocalizationDepth.OneLevel);

// localizes only root objects and all children recursively
var planets = _db.Planets.Localize<Planet>("en", LocalizationDepth.Deep);
```