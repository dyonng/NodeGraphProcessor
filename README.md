# NodeGraphProcessor

> **This is a fork of [alelievr/NodeGraphProcessor](https://github.com/alelievr/NodeGraphProcessor).**
> It exists to port the package to Unity 6.5+ and pick up a handful of upstream fixes and internal
> improvements. See [Changes in this fork](#changes-in-this-fork) below for the full list. The
> package identity was renamed from `com.alelievr.node-graph-processor` to
> `com.dyonng.node-graph-processor` (folder: `com.dyonng.NodeGraphProcessor`) — see
> [Installation](#installation) for how to pull this fork into a project instead of the original.

Node graph editor framework focused on data processing using Unity UIElements, GraphView and C# 4.7

[![Discord](https://img.shields.io/discord/823720615965622323.svg)](https://discord.gg/XuMd3Z5Rym)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4c62ece874d14a0b965b92cb163e3146)](https://www.codacy.com/manual/alelievr/NodeGraphProcessor?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=alelievr/NodeGraphProcessor&amp;utm_campaign=Badge_Grade)
[![openupm](https://img.shields.io/npm/v/com.alelievr.node-graph-processor?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.alelievr.node-graph-processor/)

This node based solution provides a great C# API allowing you to implement conditional graphs, dependencies graphs, processing graphs and more.  
![image](https://user-images.githubusercontent.com/6877923/83576832-f2486500-a532-11ea-9d2a-a6b75b980813.png)

Based on Unity's GraphView technology, NodeGraphProcessor is also very fast and works well with large graphs.  
![Performance](https://user-images.githubusercontent.com/6877923/83576843-f70d1900-a532-11ea-80fb-c8fede6aa7ed.gif)

Simple and powerful C# node API to create new nodes and custom views.

```CSharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;

[System.Serializable, NodeMenuItem("Operations/Sub")] // Add the node in the node creation context menu
public class SubNode : BaseNode
{
    [Input(name = "A")]
    public float                inputA;
    [Input(name = "B")]
    public float                inputB;

    [Output(name = "Out")]
    public float				output;

    public override string		name => "Sub";

    // Called when the graph is process, process inputs and assign the result in output.
    protected override void Process()
    {
        output = inputA - inputB;
    }
}
```

## Unity Compatible versions

This fork requires at least Unity **6000.5** (6.5) — it relies on `Object.GetEntityId()`, which
replaced the now-deprecated `GetInstanceID()` and isn't available before 6.5. If you need to
support older Unity versions, use the [upstream project](https://github.com/alelievr/NodeGraphProcessor)
instead (Unity 2020.2+, or 2019.3+ via OpenUPM).

## Installation

<details><summary>Instructions</summary>

### Install Manually
There are two ways to install this asset: you can use the Unity package manager or move the entire repo inside your Assets folder.
To install using the package manager:

- download this repo
- inside the package manager click the '+' button at the bottom to add a package from disk
- then select the package.json file located in `Assets/com.dyonng.NodeGraphProcessor`
- package is installed :)

### Install via Git

In the Package Manager, use `Add package from git URL` and paste:

```
https://github.com/dyonng/NodeGraphProcessor.git?path=Assets/com.dyonng.NodeGraphProcessor#v1.4.3
```

The `?path=` points Unity at the package subfolder, and `#v1.4.3` pins to a released tag so your
install doesn't shift under you when the branch moves. Drop the `#v1.4.3` to track `master`
directly instead (not recommended for shared projects).

Note that you'll not have access to the examples provided in this repo because the package only include the core of NodeGraphProcessor — see Install Manually above if you want the `Assets/Examples` content too.

</details>

## Changes in this fork

- **Ported to Unity 6.5 (6000.5)** — fixed compile errors from the `GetInstanceID()` →
  `GetEntityId()` migration and other Unity 6 API changes.
- **Package identity renamed** from `com.alelievr.node-graph-processor` /
  `com.alelievr.NodeGraphProcessor` to `com.dyonng.node-graph-processor` /
  `com.dyonng.NodeGraphProcessor` (folder, `package.json`, assembly names, `InternalsVisibleTo`,
  and the example graph assets' serialized type references were all updated together so existing
  graphs still deserialize correctly).
- **Cherry-picked open upstream PRs**: node-view crash/UX fixes (bad node-view rebind and a crash
  in `SyncSerializedPropertyPathes` on delete-with-connections, list-item clicks no longer trigger
  node drag), a node-rename focus-timing fix, a reflection fix so inherited
  `[CustomPortTypeBehavior]` methods on base classes are found, `BaseGraphView.CanConnectEdge` made
  `virtual` for custom edge-validation overrides, and asset-drag node creation now prioritizes the
  most-derived matching node type instead of an arbitrary insertion-order match.
- **Removed all LINQ usage** across the package (Runtime + Editor, ~130 call sites in 23 files),
  replaced with explicit loops to cut GC churn — LINQ's iterators, closures, and boxed enumerators
  were a meaningful allocation source in hot paths like port syncing and graph traversal.
- **Bug fixes found along the way**: `PortData` was being compared by reference instead of value
  (`Equals`), causing spurious port-view rebuilds on every sync; `ParameterNode` and
  `BaseGraphView` leaked event subscriptions on enable/disable and dispose; `RelayNode` could throw
  on an empty port list; a node-deletion path never removed its view from internal tracking lists;
  the `CustomPortsNode` example could crash with an out-of-range index when its output port had
  connections but its input port didn't; `package.json` declared a `samples` entry pointing at a
  path that doesn't exist for git-URL installs, which crashed Package Manager on every package-list
  refresh.
- **No more silent data loss on graph load** — if a node, edge, or exposed parameter fails to
  deserialize (renamed/deleted class, failed serialization migration, etc.), the graph used to
  silently drop it and resave without a trace. It now logs a `Debug.LogWarning` naming the graph
  asset and exact count removed, so it's visible before it overwrites the last good copy on disk.
- **Performance work**: eliminated a redundant duplicate graph-traversal build on every single edge
  edit, batched `SerializedObject`/property-path rebinding on multi-element delete (was rebuilding
  once per deleted element), removed several boxed-enumerator and per-call allocation hot spots in
  port syncing and edge-dragging, and converted a few iterator methods to eager list builds for
  better cache locality.

## Community 

Join the [NodeGraphProcessor Discord server](https://discord.gg/XuMd3Z5Rym)! 

## Features

- Node and Graph property serialization (as json)
- Scriptable Object to store graph as a Unity asset.
- Highly customizable and simple node and links API
- Support multi-input into a container (multiple float into a list of float for example)
- Graph processor which execute node's logic with a dependency order
- [Documented C# API to add new nodes / graphs](https://github.com/alelievr/NodeGraphProcessor/wiki/Node-scripting-API)
- Exposed parameters that can be set per-asset to customize the graph processing from scripts or the inspector
- Parameter set mode, you can now output data from thegraph using exposed parameters. Their values will be updated when the graph is processed
- Search window to create new nodes
- Colored groups
- Node messages (small message with it's icon beside the node)
- Stack Nodes
- Relay nodes
- Display additional settings in the inspector
- Node creation menu on edge drop
- Simplified edge connection compared to default GraphView (ShaderGraph and VFX Graph)
- Multiple graph window workflow (copy/paste)
- Vertical Ports
- Sticky notes (requires Unity 2020.1)
- Renamable nodes

More details are available [in the Changelog](CHANGELOG.md)

## Documentation

API doc is available here: [alelievr.github.io/NodeGraphProcessor](https://alelievr.github.io/NodeGraphProcessor/api/index.html)

The user manual is hosted using [Github Wiki](https://github.com/alelievr/NodeGraphProcessor/wiki).

## Remaining to do

- Investigate for ECS/Jobs integration
- API to create the graph in C#
- Subgraphs

For more details consult our [Github Project page](https://github.com/alelievr/NodeGraphProcessor/projects/2).

## Projects made with NodeGraphProcessor

### [Mixture](https://github.com/alelievr/Mixture)

[![image](https://user-images.githubusercontent.com/6877923/98482247-61239b80-2200-11eb-9d83-a1cba4cc376a.png)](https://github.com/alelievr/Mixture)

Want to be in the made with list? [Send a message to the issue #14](https://github.com/alelievr/NodeGraphProcessor/issues/14)

## Gallery

### Minimap
![](https://user-images.githubusercontent.com/6877923/90036471-6043a200-dcc3-11ea-8702-9ccc62cb0f8a.gif)

### Relay nodes
![](https://user-images.githubusercontent.com/6877923/89329982-e04c8500-d68f-11ea-8218-261225170978.gif)

### Node connection menu
![](https://user-images.githubusercontent.com/6877923/89330117-12f67d80-d690-11ea-9b62-f878b86b8342.gif)

### Node creation menu
![](https://user-images.githubusercontent.com/6877923/58935811-893adf80-876e-11e9-9f69-69ce51a432b8.png)

### Graph Parameters
![](https://user-images.githubusercontent.com/6877923/90035202-d6470980-dcc1-11ea-92e0-a754820bdc55.png)

### Groups
![](https://user-images.githubusercontent.com/6877923/58935692-3fea9000-876e-11e9-945e-8a874a4586a9.png)

### Node Settings
![](https://user-images.githubusercontent.com/6877923/71757124-c34e9a00-2e93-11ea-900c-63ecd772af3f.gif)

### Node Messages
![](https://user-images.githubusercontent.com/6877923/63230815-51dabb80-c212-11e9-9d54-382e649e77f1.png)

### Conditional Processing (in Example)
![](https://user-images.githubusercontent.com/6877923/69500269-e469b580-0ef9-11ea-9c4b-f58e793f7ecd.gif)

### Stacks
![](https://user-images.githubusercontent.com/6877923/71782933-25b4b100-2fe0-11ea-9b57-0198f7161535.gif)

### Relay Node Packing
![](https://user-images.githubusercontent.com/6877923/77270201-808aaa00-6cab-11ea-9028-e671092be194.gif)

### Node Inspector
![](https://user-images.githubusercontent.com/6877923/87306684-ac5ec380-c518-11ea-9346-1ed47e8cd016.gif)

### Improved Edge Connection
![](https://user-images.githubusercontent.com/6877923/89890139-272c0480-dbd3-11ea-86f4-696d260f707b.gif)

### Multi-Window support
![](https://user-images.githubusercontent.com/6877923/89891415-504d9480-dbd5-11ea-8b1d-873031a0677c.gif)

### Field Drawers (Thanks [@TeorikDeli](https://github.com/TeorikDeli)!)
![](https://user-images.githubusercontent.com/6877923/92417811-775f9d80-f164-11ea-9031-e6b61c98b88e.png)

### Sticky Notes (2020.1 or more required)
![image](https://user-images.githubusercontent.com/6877923/94344807-208e0b00-0022-11eb-9f93-62acd6478e30.png)

### Vertical Ports
![image](https://user-images.githubusercontent.com/6877923/106968910-199ea400-674a-11eb-8f0d-76230c3e10c5.png)

### Drag And Drop Objects
![CreateNodeFromObject](https://user-images.githubusercontent.com/6877923/110240003-20d3f000-7f4a-11eb-8adc-e52340945b74.gif)

### Renamable nodes

Just add this bit of code in your Node script to make it renamable in the UI.
```CSharp
        public override bool	isRenamable => true;
```

![RenamableNode](https://user-images.githubusercontent.com/6877923/115143209-33ac0b00-a046-11eb-88f9-3216866e3669.gif)
