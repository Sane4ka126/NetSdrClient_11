# Лабораторні з реінжинірингу (8×)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=coverage)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=bugs)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ppanchen_NetSdrClient&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=ppanchen_NetSdrClient)


Цей репозиторій використовується для курсу **реінжиніринг ПЗ**. 
Мета — провести комплексний реінжиніринг спадкового коду NetSdrClient, включаючи рефакторинг архітектури, покращення якості коду, впровадження сучасних практик розробки та автоматизацію процесів контролю якості через CI/CD пайплайни.

---

## Структура 8 лабораторних

  Кожна робота — **через Pull Request**. У PR додати короткий опис: *що змінено / як перевірити / ризики* + звіт про хід виконання в Classroom.

### Лаба 1 — Підключення SonarCloud і CI

**Мета:** створити проект у SonarCloud, підключити GitHub Actions, запустити перший аналіз.

**Необхідно:**
- .NET 8 SDK
- Публічний GitHub-репозиторій
- Обліковка SonarCloud (організація прив’язана до GitHub)

**1) Підключити SonarCloud**
- На SonarCloud створити проект з цього репозиторію (*Analyze new project*).
- Згенерувати **user token** і додати в репозиторій як секрет **`SONAR_TOKEN`** (*Settings → Secrets and variables → Actions*).
- Додати/перевірити `.github/workflows/sonarcloud.yml` з тригерами на PR і push у основну гілку.
  `sonarcloud.yml`:
```yml
# This workflow uses actions that are not certified by GitHub.
# They are provided by a third-party and are governed by
# separate terms of service, privacy policy, and support
# documentation.

# This workflow helps you trigger a SonarCloud analysis of your code and populates
# GitHub Code Scanning alerts with the vulnerabilities found.
# Free for open source project.

# 1. Login to SonarCloud.io using your GitHub account

# 2. Import your project on SonarCloud
#     * Add your GitHub organization first, then add your repository as a new project.
#     * Please note that many languages are eligible for automatic analysis,
#       which means that the analysis will start automatically without the need to set up GitHub Actions.
#     * This behavior can be changed in Administration > Analysis Method.
#
# 3. Follow the SonarCloud in-product tutorial
#     * a. Copy/paste the Project Key and the Organization Key into the args parameter below
#          (You'll find this information in SonarCloud. Click on "Information" at the bottom left)
#
#     * b. Generate a new token and add it to your Github repository's secrets using the name SONAR_TOKEN
#          (On SonarCloud, click on your avatar on top-right > My account > Security
#           or go directly to https://sonarcloud.io/account/security/)

# Feel free to take a look at our documentation (https://docs.sonarcloud.io/getting-started/github/)
# or reach out to our community forum if you need some help (https://community.sonarsource.com/c/help/sc/9)

name: SonarCloud analysis

on:
  push:
    branches: [ "master" ]
  pull_request:
    branches: [ "master" ]
  workflow_dispatch:

permissions:
  pull-requests: read # allows SonarCloud to decorate PRs with analysis results

jobs:
  sonar-check:
    name: Sonar Check
    runs-on: windows-latest   # безпечно для будь-яких .NET проектів
    steps:
      - uses: actions/checkout@v4
        with: { fetch-depth: 0 }

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      # 1) BEGIN: SonarScanner for .NET
      - name: SonarScanner Begin
        run: |
          dotnet tool install --global dotnet-sonarscanner
          echo "$env:USERPROFILE\.dotnet\tools" >> $env:GITHUB_PATH
          dotnet sonarscanner begin `
          /d:sonar.projectKey="<Project Key>" `
          /d:sonar.organization="<Organization Key>" `
          /d:sonar.token="${{ secrets.SONAR_TOKEN }}" `
          /d:sonar.cs.opencover.reportsPaths="**/coverage.xml" `
          /d:sonar.cpd.cs.minimumTokens=40 `
          /d:sonar.cpd.cs.minimumLines=5 `
          /d:sonar.exclusions=**/bin/**,**/obj/**,**/sonarcloud.yml `
          /d:sonar.qualitygate.wait=true
        shell: pwsh
      # 2) BUILD & TEST
      - name: Restore
        run: dotnet restore NetSdrClient.sln
      - name: Build
        run: dotnet build NetSdrClient.sln -c Release --no-restore
      #- name: Tests with coverage (OpenCover)
      #  run: |
      #    dotnet test NetSdrClientAppTests/NetSdrClientAppTests.csproj -c Release --no-build `
      #      /p:CollectCoverage=true `
      #      /p:CoverletOutput=TestResults/coverage.xml `
      #      /p:CoverletOutputFormat=opencover
      #  shell: pwsh
      # 3) END: SonarScanner
      - name: SonarScanner End
        run: dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
        shell: pwsh
```
        
- **Вимкнути Automatic Analysis** в проєкті.
- Перевірити **PR-декорацію** (вкладка *Checks* у PR).

**Здати:** посилання на PR із зеленим аналізом, скрін Quality Gate, скрін бейджів у README.

---

### Лаба 2 — Code Smells через PR + “gated merge”

**Мета:** виправити **5–10** зауважень Sonar (bugs/smells) без зміни поведінки.

**Кроки:**
- Дрібними комітами виправити знайдені Sonar-проблеми у `NetSdrClientApp`.

**Здати:** PR із “зеленими” required checks; скріни змін метрик у Sonar.

---

### Лаба 3 — Тести та покриття

**Мета:** підняти покриття коду юніт-тестами в модулі.

**Кроки:**
- Підключити генерацію покриття:
  - `coverlet.msbuild`:
    ```bash
    dotnet add NetSdrClientAppTests package coverlet.msbuild
    dotnet add NetSdrClientAppTests package Microsoft.NET.Test.Sdk
    dotnet test NetSdrClientAppTests -c Release       /p:CollectCoverage=true       /p:CoverletOutput=TestResults/coverage.xml       /p:CoverletOutputFormat=opencover
    ```
- У Sonar додати крок запуску тестів:
  ```
  - name: Tests with coverage (OpenCover)
    run: |
      dotnet test NetSdrClientAppTests/NetSdrClientAppTests.csproj -c Release --no-build `
        /p:CollectCoverage=true `
        /p:CoverletOutput=TestResults/coverage.xml `
        /p:CoverletOutputFormat=opencover
    shell: pwsh
  ```
- додати 4–6 юніт-тестів

**Здати:** PR із новими тестами, скрін Coverage у Sonar.

---

### Лаба 4 — Дублікати через SonarCloud

**Мета:** зменшити дублікати коду.

**Кроки:**
- Переглянути **Measures → Duplications** у Sonar і **Checks → SonarCloud** у PR.
- Прибрати **1–2** найбільші дубльовані фрагменти (рефакторинг/винесення спільного коду).
- Перезапустити CI, перевірити, що *Duplications on New Code* ≤ порога (типово 3%).

**Здати:** PR із зеленим Gate і скрінами “до/після”.

---

### Лаба 5 — Архітектурні правила (NetArchTest)

**Мета:** дослідження архітектурних правила залежностей

**Кроки:**
- Додати кілька архітектурних правил залежностей (наприклад, `*.UI` не має залежати від `*.Infrastructure` напряму).
- Переконатися, що порушення **ламає збірку** (червоний PR), а фікс — зеленить.

**Здати:** PR із тестами правил, скрін невдалого прогону (до фіксу) і зеленого (після).

---

### Лаба 6 — Безпечний рефакторинг під тести

**Мета:** рефакторинг коду

**Кроки:**
- Додати проект з юніт тестами для `EchoServer`
- Реалізувати необхідні зміни в `EchoServer` для покращення його придатності до тестування
- Покрити код юніт-тестами

**Здати:** PR + коротка таблиця метрик “до/після”.

---

### Лаба 7 — Оновлення залежностей

**Мета:**навчитись виявляти й виправляти уразливі залежності, користуватись інструментами GitHub Security (Dependency graph, Dependabot alerts/updates).

**Кроки:**
- `dotnet list NetSdrClient.sln package --outdated --include-transitive`
- Увімкнути GitHub Security
  - Repo → Settings → Code security and analysis → включи Dependency graph + Dependabot alerts.
  - Через кілька хвилин GitHub має показати алерт про Newtonsoft.Json.

- Налаштувати Dependabot
  - Додай у корінь .github/dependabot.yml:
```
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
```   
  - Оновити обрані пакети, прогнати тест/сонар. Dependabot створить PR на оновлення до безпечної версії (13.0.1+).

**Здати:** PR з оновленням, скрін push-рану після мерджу, нотатки про ризики.

---

### Лаба 8 — Чистий проєкт і gated build

**Мета:** Домогтися зеленого Quality Gate у SonarCloud. Увімкнути gated merge у GitHub

**Кроки:**
- Довести SonarCloud до “зеленого”
  - Пройти всі умови Quality Gate (типово “Sonar way”), зокрема на New Code:
  - Bugs/Vulnerabilities = 0 (на новому коді).
  - Coverage on New Code ≥ 80% (підняти тести).
  - Duplications on New Code ≤ 3% (або твій суворіший поріг).
  - Code Smells: критичні — виправити; інші — зменшити.
  - Security Hotspots: переглянути й закрити/виправити.
- Увімкнути gated merge у GitHub
  - Repo → Settings → Branches → Add rule для main:
  - Require a pull request before merging
  - Require status checks to pass → відміть:
    - твій CI-джоб (наприклад, CI / Tests & Sonar)
    - SonarCloud Code Analysis / SonarCloud Quality Gate
  - (Опц.) Require approvals (1–2)
  - (Опц.) Require branches to be up to date (щоб ребейзилися перед мерджем)
**Здати:** скрін *Branches → main* з зеленим Gate

---

## Норми здачі та оцінювання (єдині для всіх лаб)

**Подання:** лише через **Pull Request**.  
**Опис PR:** що зроблено, як перевірити, ризики/зворотна сумісність.  
**Артефакти:** скріни/посилання на Sonar, логи CI, coverage report.  
**Критерій “зелений PR”:** CI пройшов, **Quality Gate** зелений, покриття/дублі в нормі.

---

## Типові граблі → що робити

- **“You are running CI analysis while Automatic Analysis is enabled”**
  Вимкнути *Automatic Analysis* у SonarCloud (використовуємо CI).
- **“Project not found”**
  Перевірити `sonar.organization`/`sonar.projectKey` **точно як у UI**; токен має доступ до org.
- **Покриття не генерується**
  Додати `coverlet.msbuild` або `coverlet.collector`; використовувати формат **opencover**; у Sonar — `sonar.cs.opencover.reportsPaths`.
- **Подвійний аналіз (PR + push)**
  Обмежити умову запуску Sonar: тільки PR **або** `refs/heads/master`.
- **PR зелений, push червоний**
  Перевірити **New Code Definition** (Number of days або Previous version) і довести покриття/дублікації на “new code”.
