# Configuration Setup Guide

## Security Best Practices

This project follows security best practices by **NOT committing sensitive configuration files** to the repository.

## Configuration Files

### Committed to Repository ✅
- **`appsettings.json`** - Base configuration with placeholder values (safe to commit)
- **`appsettings.Development.json.example`** - Example development configuration (safe to commit)

### NOT Committed (In .gitignore) 🔒
- **`appsettings.Development.json`** - Your actual development settings with real credentials
- **`appsettings.Production.json`** - Production settings with real secrets
- **`appsettings.*.json`** - Any environment-specific configuration

## First Time Setup

When you clone this repository, follow these steps:

### 1. Create Your Development Configuration

```bash
cd src/UI.API
copy appsettings.Development.json.example appsettings.Development.json
```

### 2. Update Settings in `appsettings.Development.json`

Replace all placeholder values with your actual credentials:

#### Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourDbName;Trusted_Connection=True;"
}
```

#### Email Configuration
For development, use [Mailtrap](https://mailtrap.io) (free testing email service):
```json
"EmailConfig": {
  "Host": "smtp.mailtrap.io",
  "SmtpPort": 587,
  "Email": "your-mailtrap-username",
  "Password": "your-mailtrap-password",
  "UseSSL": true
}
```

#### JWT Secret
Generate a secure random string (minimum 32 characters):
```bash
# PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})

# Or use an online generator
# https://randomkeygen.com/
```

```json
"Jwt": {
  "Secret": "your-generated-secret-key-here-min-32-chars"
}
```

#### OAuth Providers (Optional)

If you want to enable social login:

**Google OAuth:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URIs: `https://localhost:5001/api/auth/v1/auth/external-login-callback`

**Facebook OAuth:**
1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app
3. Add Facebook Login product
4. Configure redirect URIs

```json
"Authentication": {
  "Google": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret"
  },
  "Facebook": {
    "AppId": "your-app-id",
    "AppSecret": "your-app-secret"
  }
}
```

## Production Setup

For production, create `appsettings.Production.json` with:

- Production database connection string
- Production email server
- Strong JWT secret (different from development)
- Production OAuth credentials
- Azure Storage connection (for data protection keys)
- Application Insights connection string

**IMPORTANT:** Never commit production credentials to the repository!

## Environment Variables (Alternative)

Instead of using `appsettings.*.json` files, you can use environment variables:

```bash
# Example for connection string
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=ProdDb;..."

# Example for JWT secret
export Jwt__Secret="production-secret-key"
```

ASP.NET Core automatically maps environment variables with `__` (double underscore) to configuration sections.

## User Secrets (Development Alternative)

For development, you can use .NET User Secrets instead of `appsettings.Development.json`:

```bash
cd src/UI.API

# Set secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "Jwt:Secret" "your-jwt-secret"
dotnet user-secrets set "EmailConfig:Password" "your-email-password"

# List all secrets
dotnet user-secrets list
```

User secrets are stored outside the project directory and are never committed.

## Configuration Priority

ASP.NET Core loads configuration in this order (later sources override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. User Secrets (development only)
4. Environment Variables (deployment/container)
5. Command-line arguments

## Troubleshooting

### "Connection string not found"
- Ensure `appsettings.Development.json` exists
- Verify `ASPNETCORE_ENVIRONMENT` is set to "Development"
- Check connection string format

### "Invalid JWT secret"
- JWT secret must be at least 32 characters
- Ensure no special characters that need escaping in JSON

### Email not sending
- For development, use Mailtrap or similar testing service
- Check SMTP port (usually 587 for TLS, 465 for SSL)
- Verify firewall isn't blocking outbound SMTP

### OAuth redirect issues
- Ensure redirect URIs match exactly in provider console
- Use HTTPS in production (OAuth providers require it)
- Check that ports match (especially in development)

## Security Checklist

Before deploying or committing:

- [ ] No real credentials in `appsettings.json`
- [ ] `appsettings.Development.json` is in `.gitignore`
- [ ] `appsettings.Production.json` is in `.gitignore`
- [ ] All secrets use placeholders in committed files
- [ ] JWT secret is strong and unique per environment
- [ ] Production secrets are stored securely (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Database passwords are complex and rotated regularly
- [ ] OAuth secrets are not exposed in client-side code

## Need Help?

If you encounter issues with configuration:

1. Check this guide first
2. Review the example file: `appsettings.Development.json.example`
3. Check the README.md configuration section
4. Open an issue on GitHub with **sanitized** config (remove real secrets!)
