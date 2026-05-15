# Reference HTML captures

This directory contains anonymized HTML pages captured from the web service.

## Purpose

These files are used for interoperability testing and bug diagnosis in the web service. 
When the service changes its page structure or form fields, these
captures help identify what changed and guide client-side fixes without requiring a live connection.

## Privacy and security

All captures are sanitized before being committed:

- No personal data (names, emails, addresses, dates of birth)
- No authentication tokens, session cookies, or CSRF tokens
- No real numeric user or company IDs
- No API keys or error-tracking DSNs
- JavaScript is disabled (`type="text/plain"` on all script tags + CSP meta tag)

Sanitization is performed by [`Sanitize-HtmlRef.ps1`](Sanitize-HtmlRef.ps1) in this directory.
Original unsanitized files are kept locally as `*.html.orig` and excluded from version control
via `.gitignore`.

## Legal

These captures are retained under the interoperability provisions of EU Software Directive
2009/24/EC (Article 6) to enable compatibility work. They contain no proprietary data beyond
the publicly visible form structure accessible to any authenticated user of the service.

## Adding a new capture

1. Save the page from your browser as *Web Page, HTML only* into the appropriate subdirectory
2. Run: `./ref/Sanitize-HtmlRef.ps1 <path/to/file.html>`
3. Verify the output looks correct, then commit the sanitized file
