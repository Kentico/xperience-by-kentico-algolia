# Xperience by Kentico Algolia

[![7-day bug-fix policy](https://img.shields.io/badge/-7--days_bug--fixing_policy-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik04ODguNDkgMjIyLjY4NnYtMzEuNTRsLTY1LjY3Mi0wLjk1NWgtMC4yMDVhNDY1LjcxNSA0NjUuNzE1IDAgMCAxLTE0NC4zMTUtMzEuMzM0Yy03Ny4wMDUtMzEuMTk4LTEyNi4yOTQtNjYuNzY1LTEyNi43MDMtNjcuMTA3bC0zOS44LTI4LjY3Mi0zOS4xODUgMjguNDY4Yy0yLjA0OCAxLjUwMS00OS45MDMgMzYuMDQ0LTEyNi45MDggNjcuMzFhNDQ3LjQyIDQ0Ny40MiAwIDAgMS0xNDQuNTIgMzEuMzM1bC02NS44NzcgMC45NTZ2Mzc4Ljg4YzAgODcuMDQgNDkuODM0IDE4NC42NjEgMTM3LjAxIDI2Ny44MSAzNy41NDcgMzUuODQgNzkuMjU4IDY2LjM1NSAxMjAuODMzIDg4LjIgNDMuMjggMjIuNzMzIDg0LjI0IDM0LjYxMiAxMTguODUyIDM0LjYxMiAzNC40MDYgMCA3NS43NzYtMTIuMTUyIDExOS42MDMtMzUuMTU4YTU0Ny45NzcgNTQ3Ljk3NyAwIDAgMCAxMjAuMDEzLTg3LjY1NCA1MTUuMjA5IDUxNS4yMDkgMCAwIDAgOTYuMTg4LTEyMi44OGMyNy4xMDItNDkuNTYyIDQwLjgyMy05OC4zMDQgNDAuODIzLTE0NC45OTlsLTAuMTM2LTM0Ny4yMDR6TTUxMC4wOSAxNDMuNDI4bDEuNzA2LTEuMzY1IDEuNzc1IDEuMzY1YzUuODAzIDQuMTY1IDU5LjUyOSA0MS44NDggMTQwLjM1NiA3NC43NTIgNzkuMTkgMzIuMDg2IDE1My42IDM1LjYzNSAxNjcuNjYzIDM2LjA0NWwyLjU5NCAwLjA2OCAwLjIwNSAzMTUuNzM0YzAuMTM3IDY5LjQ5NS00Mi41OTggMTUwLjE4Ni0xMTcuMDc3IDIyMS40NTdDNjQxLjU3IDg1NC4yODkgNTYzLjEzIDg5Ni40NzggNTEyIDg5Ni40NzhjLTIzLjY4OSAwLTU1LjU3LTkuODk5LTg5LjcwMi0yNy43ODVhNDc4LjgyMiA0NzguODIyIDAgMCAxLTEwNS42MDktNzcuMjc4QzI0Mi4yMSA3MjAuMjEzIDE5OS40NzUgNjM5LjUyMiAxOTkuNDc1IDU2OS44OVYyNTQuMjI1bDIuNzMtMC4xMzZjMy4yNzggMCA4Mi42MDQtMS41MDIgMTY3LjY2NC0zNS45NzdhNzM5Ljk0MiA3MzkuOTQyIDAgMCAwIDE0MC4yMi03NC42MTV2LTAuMDY5eiIgIC8+PHBhdGggZD0iTTcxMy4zMTggMzY4LjY0YTMyLjIyMiAzMi4yMjIgMCAwIDAtNDUuMzI5IDBMNDQ5LjE5NSA1ODcuNDM1bC05My4xODQtOTMuMTE2YTMyLjIyMiAzMi4yMjIgMCAwIDAtNDUuMzMgMCAzMi4yMjIgMzIuMjIyIDAgMCAwIDAgNDUuMjZsMTE1Ljg1IDExNS44NWEzMi4yOSAzMi4yOSAwIDAgMCA0NS4zMjggMEw3MTMuMzIgNDEzLjlhMzIuMjIyIDMyLjIyMiAwIDAgMCAwLTQ1LjMzeiIgIC8+PC9zdmc+)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) [![CI: Build and Test](https://github.com/Kentico/xperience-by-kentico-algolia/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Kentico/xperience-by-kentico-algolia/actions/workflows/ci.yml) [![NuGet Package](https://img.shields.io/nuget/v/Kentico.Xperience.Algolia.svg)](https://www.nuget.org/packages/Kentico.Xperience.Algolia)

## Description

This integration enables you to create [Algolia](https://www.algolia.com/) search indexes to index content of pages ([content types](https://docs.xperience.io/x/gYHWCQ) with the 'Page' feature enabled) from the Xperience content tree using a code-first approach. To provide a search interface for the indexed content, developers can use the [.NET API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/csharp/?client=csharp), [JavaScript API](https://www.algolia.com/doc/api-client/getting-started/what-is-the-api-client/javascript/?client=javascript), or the [InstantSearch.js](https://www.algolia.com/doc/guides/building-search-ui/what-is-instantsearch/js/) library.

## Library Version Matrix

| Xperience Version   | Library Version |
| ------------------- | --------------- |
| 28.x                | 3.x             |
| >= 26.2.0, < 27.0.0 | 2.x             |

## Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.xperience.io/xp/changelog)
- [Algolia](https://www.algolia.com/doc/)
## Package Installation

Add the package to your application using the .NET CLI

```powershell
dotnet add package Kentico.Xperience.Algolia
```

## Quick Start

1. Add configuration from your Algolia account to the ASP.NET Core `appsettings.json` file:

   ```json
   "CMSAlgoliaSearch": {
       "ApplicationId": "<your application ID>",
       "ApiKey": "<your Indexing API key>",
       "SearchKey": "<your Search API key>"
   }
   ```

2. Define a custom `DefaultAlgoliaIndexingStrategy` implementation to customize how content pages/content items are processed for the index. See [`Custom-index-strategy.md`](docs/Custom-index-strategy.md)
3. Add this library to the application services, registering your custom `DefaultAlgoliaIndexingStrategy` and Algolia services

   ```csharp
   // Program.cs
   services.AddKenticoAlgolia(configuration);
   services.RegisterStrategy<GlobalAlgoliaIndexingStrategy>("DefaultStrategy");
   ```

4. Create an index in Xperience's Administration within the Search application added by this library.
![Administration search edit form](/images/xperience-administration-search-index-edit-form.jpg)
1. Rebuild the index in Xperience's Administration within the Search application added by this library.
![Administration search edit form](/images/xperience-administration-search-index-list.jpg)
1. Display the results on your site with a Razor View 👍.

## Full Instructions

View the [Usage Guide](docs/Usage-Guide.md) for more detailed instructions.

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](LICENSE.md) for more information.

## Support

[![7-day bug-fix policy](https://img.shields.io/badge/-7--days_bug--fixing_policy-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik04ODguNDkgMjIyLjY4NnYtMzEuNTRsLTY1LjY3Mi0wLjk1NWgtMC4yMDVhNDY1LjcxNSA0NjUuNzE1IDAgMCAxLTE0NC4zMTUtMzEuMzM0Yy03Ny4wMDUtMzEuMTk4LTEyNi4yOTQtNjYuNzY1LTEyNi43MDMtNjcuMTA3bC0zOS44LTI4LjY3Mi0zOS4xODUgMjguNDY4Yy0yLjA0OCAxLjUwMS00OS45MDMgMzYuMDQ0LTEyNi45MDggNjcuMzFhNDQ3LjQyIDQ0Ny40MiAwIDAgMS0xNDQuNTIgMzEuMzM1bC02NS44NzcgMC45NTZ2Mzc4Ljg4YzAgODcuMDQgNDkuODM0IDE4NC42NjEgMTM3LjAxIDI2Ny44MSAzNy41NDcgMzUuODQgNzkuMjU4IDY2LjM1NSAxMjAuODMzIDg4LjIgNDMuMjggMjIuNzMzIDg0LjI0IDM0LjYxMiAxMTguODUyIDM0LjYxMiAzNC40MDYgMCA3NS43NzYtMTIuMTUyIDExOS42MDMtMzUuMTU4YTU0Ny45NzcgNTQ3Ljk3NyAwIDAgMCAxMjAuMDEzLTg3LjY1NCA1MTUuMjA5IDUxNS4yMDkgMCAwIDAgOTYuMTg4LTEyMi44OGMyNy4xMDItNDkuNTYyIDQwLjgyMy05OC4zMDQgNDAuODIzLTE0NC45OTlsLTAuMTM2LTM0Ny4yMDR6TTUxMC4wOSAxNDMuNDI4bDEuNzA2LTEuMzY1IDEuNzc1IDEuMzY1YzUuODAzIDQuMTY1IDU5LjUyOSA0MS44NDggMTQwLjM1NiA3NC43NTIgNzkuMTkgMzIuMDg2IDE1My42IDM1LjYzNSAxNjcuNjYzIDM2LjA0NWwyLjU5NCAwLjA2OCAwLjIwNSAzMTUuNzM0YzAuMTM3IDY5LjQ5NS00Mi41OTggMTUwLjE4Ni0xMTcuMDc3IDIyMS40NTdDNjQxLjU3IDg1NC4yODkgNTYzLjEzIDg5Ni40NzggNTEyIDg5Ni40NzhjLTIzLjY4OSAwLTU1LjU3LTkuODk5LTg5LjcwMi0yNy43ODVhNDc4LjgyMiA0NzguODIyIDAgMCAxLTEwNS42MDktNzcuMjc4QzI0Mi4yMSA3MjAuMjEzIDE5OS40NzUgNjM5LjUyMiAxOTkuNDc1IDU2OS44OVYyNTQuMjI1bDIuNzMtMC4xMzZjMy4yNzggMCA4Mi42MDQtMS41MDIgMTY3LjY2NC0zNS45NzdhNzM5Ljk0MiA3MzkuOTQyIDAgMCAwIDE0MC4yMi03NC42MTV2LTAuMDY5eiIgIC8+PHBhdGggZD0iTTcxMy4zMTggMzY4LjY0YTMyLjIyMiAzMi4yMjIgMCAwIDAtNDUuMzI5IDBMNDQ5LjE5NSA1ODcuNDM1bC05My4xODQtOTMuMTE2YTMyLjIyMiAzMi4yMjIgMCAwIDAtNDUuMzMgMCAzMi4yMjIgMzIuMjIyIDAgMCAwIDAgNDUuMjZsMTE1Ljg1IDExNS44NWEzMi4yOSAzMi4yOSAwIDAgMCA0NS4zMjggMEw3MTMuMzIgNDEzLjlhMzIuMjIyIDMyLjIyMiAwIDAgMCAwLTQ1LjMzeiIgIC8+PC9zdmc+)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support)

This project has **Full support by 7-day bug-fix policy**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

## Security

For any security issues see [Kentico's `SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
