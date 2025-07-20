# AI HelpDesk

**Self‑hosted AI HelpDesk assistant** for enterprise teams — single‑tenant, white‑label, customizable, built on .NET 8 & Blazor Server.

---

[![CI](https://github.com/MatteoRigoni/AIHelpDesk/actions/workflows/ci.yml/badge.svg)](https://github.com/MatteoRigoni/AIHelpDesk/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Stars](https://img.shields.io/github/stars/MatteoRigoni/AIHelpDesk?style=social)](https://github.com/MatteoRigoni/AIHelpDesk/stargazers)

> A modern, on‑premise AI assistant you can install and brand for each customer. Fast to deploy on Docker or Azure App Service, with Qdrant vector search and GPT‑4 powered RAG.

## 🔍 Features

- **Single‑tenant**: one instance per customer, on Azure, AWS or on‑premise  
- **Clean Architecture**: Domain / Application / Infrastructure / WebUI  
- **Auth & Roles**: Identity with Admin & HelpDeskAgent roles  
- **Document RAG**: upload PDF/TXT/CSV → parse → chunk → embeddings → Qdrant k‑NN → GPT‑4  
- **Admin Panel**: branding, prompt & parameter customization, user management, document dashboard  
- **Chat UI**: MudBlazor chat interface with history & export  
- **Logging & History**: persistent chat logs with filtering by role  
- **Easy Deploy**: Dockerfile + `docker-compose.yml` + Azure App Service-ready scripts  