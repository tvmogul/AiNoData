# AiNoData™
My name is **Bill SerGio** and I am the inventor of **Decision Space™**, a new mathematical construct for decision-making. This work introduces a mathematical framework defining a **Decision Space™** where candidate actions or states are evaluated, scored, and selected using deterministic cost, energy, and risk functions, without learned weights, probabilities, or historical data.

![AiNoData](https://ainodata.com/img/og-image.png)

# Zero-Training AI™ — Interactive Demos

**Four deterministic, real-time decision systems that do not use training data, datasets, or machine learning.**

Watch Video : https://www.youtube.com/watch?v=7_3RRghur1k

This repository contains a set of interactive web-based demonstrations showcasing **Zero-Training AI™** — a class of decision and optimization systems that operate through **mathematical evaluation at runtime**, rather than statistical learning from data.

At the core of this work is my creation of a **new mathematical framework for decision-making**, including the formalization of a **Decision Space™**: a structured space in which candidate actions or states are evaluated, scored, and selected based on deterministic cost, energy, and risk functions.  
Decisions emerge from direct evaluation within this Decision Space™, not from learned weights, probabilities, or historical examples.

These demos are intentionally simple, visual, and self-contained, designed to show how certain classes of problems can be solved without training, models, or inference pipelines.

---

## 🚀 What Is Zero-Training AI™?

Zero-Training AI™ refers to systems that:

- Require **no training data**
- Use **no machine learning models**
- Perform **real-time evaluation and selection**
- Produce **deterministic, explainable outcomes**
- Remain stable under changing inputs and constraints

Instead of learning from historical examples, these systems operate by:

- Evaluating candidate states or actions within a defined **Decision Space**
- Applying mathematical cost, energy, or risk functions
- Selecting stable or optimal configurations at runtime

This approach is particularly useful in domains where:

- Decisions must be made instantly
- Training data is unavailable, unreliable, or dangerous
- Explainability and determinism matter
- Post-generation control is required

---

## ⚖️ Legal & Usage Notes

- These demos are provided for **educational and illustrative purposes only**
- They are **not production systems**
- They do **not** model real-world physics, medical devices, financial instruments, or autonomous vehicles
- Visual motion and scaling represent **abstract indicators**, not physical energy or force

This repository does **not** constitute an offer to sell, a solicitation to buy, or a solicitation of investment interest in any security or product.

See the included disclaimer for full details.

---

## 📄 License & Intellectual Property

Source code in this repository is provided under an open-source license (see `LICENSE` file).

**Zero-Training AI™**, the underlying mathematical framework, and the concept of **Decision Space** may be protected by trademark and/or patent rights.  
Use of the name does not imply transfer of ownership or rights.

---

## 🧠 Who This Is For

- Software engineers
- Control systems developers
- AI researchers
- System architects
- Skeptics of data-heavy ML pipelines
- Anyone interested in **deterministic alternatives to machine learning**

---

## 📘 Related Article

This repository accompanies an article discussing the concepts and tradeoffs behind **Zero-Training AI™**, including when machine learning is *not* the right tool.

https://ainodata.com/

---

## 📬 Feedback

Questions, issues, and technical discussion are welcome via GitHub Issues.

---

## 🧪 Included Demos

### 1. **Budget Allocator**
**Zero-training optimization under financial constraints**

Automatically allocates a fixed starting budget across many competing media-buy options to maximize net profit.

**Demonstrates:**
- Constraint-based optimization
- Deterministic selection within a Decision Space
- Compounding decision logic
- No forecasting models or training loops

---

### 2. **Drone Hover Stabilizer Simulation**
**Real-time stabilization without physics simulation or learning**

A simplified quad-drone icon attempts to remain level while responding instantly to disturbances such as wind gusts or tilt.

**Demonstrates:**
- Real-time corrective control
- Stable convergence without learning
- Visual indicators of relative control effort (not physical energy)

⚠️ *This is a conceptual visualization, not a flight simulator.*

---

### 3. **Robot Arm Balancer**
**Pure inverse-kinematics control with smooth convergence**

A 2-joint planar robot arm continuously adjusts to reach a user-controlled target point, finding a stable configuration without training.

**Demonstrates:**
- Deterministic inverse kinematics
- Continuous re-optimization within a Decision Space
- Smooth, damped convergence
- No datasets, heuristics, or machine learning

---

### 4. **LLM Token Governor (Hallucination Eliminator)**
**Post-generation control for large language models**

Evaluates multiple candidate sentences produced by an LLM and selects the lowest-risk, lowest-energy statement based on structural language signals.

**Demonstrates:**
- AI governance without retraining models
- Runtime filtering of LLM outputs
- Domain-sensitive risk weighting
- No prompt tuning or fine-tuning

---

## 🛠️ Technology Stack

- ASP.NET Core MVC  
- Razor Views  
- JavaScript / HTML5 Canvas  
- Client-side real-time mathematics  
- No external AI frameworks  
- No ML libraries  
- No cloud dependencies required  

All demos run locally once the app is started.

---

## 🧭 Project Structure

```text
AiNoData/
│
├── Controllers/
│   ├── BudgetController.cs
│   ├── DroneController.cs
│   ├── RobotController.cs
│   └── LlmGovernorController.cs
│
├── Views/
│   ├── Budget/
│   ├── Drone/
│   ├── Robot/
│   └── LlmGovernor/
│
├── Views/Shared/
│   └── _Disclaimer.cshtml
│
├── Models/
│   ├── Budget/
│   │   ├── BudgetAllocatorViewModel.cs
│   │   ├── MediaChannelAllocationResult.cs
│   │   ├── MediaChannelInput.cs
│   │   └── MonthlyAllocationSnapshot.cs
│   │
│   └── Drone/
│       ├── DroneState.cs
│       ├── DroneControlInput.cs
│       ├── DroneSimulationSnapshot.cs
│       └── DroneEnvironmentParameters.cs
│
├── Services/
│   ├── Budget/
│   │   ├── IZ3DAllocatorService.cs
│   │   └── Z3DAllocatorService.cs
│   │
│   └── Drone/
│       ├── IDroneZ3DService.cs
│       └── DroneZ3DService.cs
│
├── wwwroot/
│   ├── js/
│   ├── css/
│   └── img/
│
└── README.md

 intentionally simple, visual, and self-contained, designed to show how certain classes of problems can be solved without training, models, or inference pipelines.
