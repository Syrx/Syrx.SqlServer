---
name: powershell-cmdlet-development-guidelines
applyTo: '**/*.ps1,**/*.psm1'
description: 'Consolidated PowerShell scripting and cmdlet guidance for safe, idiomatic automation.'
---

# PowerShell Cmdlet Development Guidelines

This guide provides PowerShell-specific instructions to help GitHub Copilot generate idiomatic,
safe, and maintainable scripts. It aligns with Microsoft cmdlet development guidance while staying concise enough for day-to-day use.

## Naming Conventions

- **Verb-Noun Format:**
  - Use approved PowerShell verbs (Get-Verb)
  - Use singular nouns
  - PascalCase for both verb and noun
  - Avoid special characters and spaces

- **Parameter Names:**
  - Use PascalCase
  - Choose clear, descriptive names
  - Use singular form unless always multiple
  - Follow PowerShell standard names

- **Variable Names:**
  - Use PascalCase for public variables
  - Use camelCase for private variables
  - Avoid abbreviations
  - Use meaningful names

- **Alias Avoidance:**
  - Use full cmdlet names
  - Avoid using aliases in scripts (e.g., use Get-ChildItem instead of gci)
  - Document any custom aliases
  - Use full parameter names

## Parameters

- Use validation attributes such as `ValidateNotNullOrEmpty` and `ValidateSet`.
- Use `[switch]` for flags rather than Boolean parameters.
- Prefer automation-friendly parameter sets over interactive prompts.

## Pipeline and Output

- Support `ValueFromPipeline` where it is natural.
- Stream objects from `process` rather than collecting large arrays.
- Return objects, not formatted text.
- Avoid `Write-Host` for data.

## Error Handling and Safety

- Use `[CmdletBinding(SupportsShouldProcess = $true)]` for mutating operations.
- Prefer `$PSCmdlet.WriteError()` or `$PSCmdlet.ThrowTerminatingError()` in advanced functions.
- Use `Write-Verbose`, `Write-Warning`, and `Write-Error` appropriately.
- Avoid `Read-Host` in automation-oriented scripts.

## General Rules

- Use full cmdlet names rather than aliases.
- Prefer clear, typed objects and structured output.
- Keep scripts safe for unattended execution.
        [string]$Username,

        [Parameter()]
        [switch]$Force
    )

    begin {
        Write-Verbose 'Starting user account removal process'
        $ErrorActionPreference = 'Stop'
    }

    process {
        try {
            # Validation
            if (-not (Test-UserExists -Username $Username)) {
                $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                    [System.Exception]::new("User account '$Username' not found"),
                    'UserNotFound',
                    [System.Management.Automation.ErrorCategory]::ObjectNotFound,
                    $Username
                )
                $PSCmdlet.WriteError($errorRecord)
                return
            }

            # Confirmation
            $shouldProcessMessage = "Remove user account '$Username'"
            if ($Force -or $PSCmdlet.ShouldProcess($Username, $shouldProcessMessage)) {
                Write-Verbose "Removing user account: $Username"

                # Main operation
                Remove-ADUser -Identity $Username -ErrorAction Stop
                Write-Warning "User account '$Username' has been removed"
            }
        } catch [Microsoft.ActiveDirectory.Management.ADException] {
            $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                $_.Exception,
                'ActiveDirectoryError',
                [System.Management.Automation.ErrorCategory]::NotSpecified,
                $Username
            )
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        } catch {
            $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                $_.Exception,
                'UnexpectedError',
                [System.Management.Automation.ErrorCategory]::NotSpecified,
                $Username
            )
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        }
    }

    end {
        Write-Verbose 'User account removal process completed'
    }
}
```

## Documentation and Style

- **Comment-Based Help:** Include comment-based help for any public-facing function or cmdlet. Inside the function, add a `<# ... #>` help comment with at least:
  - `.SYNOPSIS` Brief description
  - `.DESCRIPTION` Detailed explanation
  - `.EXAMPLE` sections with practical usage
  - `.PARAMETER` descriptions
  - `.OUTPUTS` Type of output returned
  - `.NOTES` Additional information

- **Consistent Formatting:**
  - Follow consistent PowerShell style
  - Use proper indentation (4 spaces recommended)
  - Opening braces on same line as statement
  - Closing braces on new line
  - Use line breaks after pipeline operators
  - PascalCase for function and parameter names
  - Avoid unnecessary whitespace

- **Pipeline Support:**
  - Implement Begin/Process/End blocks for pipeline functions
  - Use ValueFromPipeline where appropriate
  - Support pipeline input by property name
  - Return proper objects, not formatted text

- **Avoid Aliases:** Use full cmdlet names and parameters
  - Avoid using aliases in scripts (e.g., use Get-ChildItem instead of gci); aliases are acceptable for interactive shell use.
  - Use `Where-Object` instead of `?` or `where`
  - Use `ForEach-Object` instead of `%`
  - Use `Get-ChildItem` instead of `ls` or `dir`

## Full Example: End-to-End Cmdlet Pattern

```powershell
function New-Resource {
    [CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
    param(
        [Parameter(Mandatory = $true,
            ValueFromPipeline = $true,
            ValueFromPipelineByPropertyName = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Name,

        [Parameter()]
        [ValidateSet('Development', 'Production')]
        [string]$Environment = 'Development'
    )

    begin {
        Write-Verbose 'Starting resource creation process'
    }

    process {
        try {
            if ($PSCmdlet.ShouldProcess($Name, 'Create new resource')) {
                # Resource creation logic here
                Write-Output ([PSCustomObject]@{
                        Name        = $Name
                        Environment = $Environment
                        Created     = Get-Date
                    })
            }
        } catch {
            $errorRecord = [System.Management.Automation.ErrorRecord]::new(
                $_.Exception,
                'ResourceCreationFailed',
                [System.Management.Automation.ErrorCategory]::NotSpecified,
                $Name
            )
            $PSCmdlet.ThrowTerminatingError($errorRecord)
        }
    }

    end {
        Write-Verbose 'Completed resource creation process'
    }
}
```
