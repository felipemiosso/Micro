# User Invitations & Role Assignment

## Overview
Admins need to invite team members to the ATS and assign specific roles to them, controlling their access to requisitions, candidates, and administrative settings. 

## User Stories
- **As an Admin**, I want to send email invitations to new team members, so they can access the ATS.
- **As an Admin**, I want to select roles for the invited user during the invitation process, so they have the correct permissions upon joining.
- **As an Admin**, I want to view a list of pending invitations and have the ability to revoke them.
- **As an Invitee**, I want to click a secure link in my email to set my password and activate my account.
- **As an Admin**, I want to manage roles for existing active users from a User Management page.

## Open Questions
- **Resolved**: We will use Firebase Admin SDK to provision the account immediately and send a standard Firebase "Password Reset" email to act as the invitation. This avoids needing a third-party email provider for MVP.
- Does an invitation expire? Standard Firebase password reset links expire based on Firebase project settings (usually 1 hour). We will track "Invite Sent" state locally.
