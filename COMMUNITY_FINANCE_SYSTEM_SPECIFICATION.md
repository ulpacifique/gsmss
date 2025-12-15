# Community Finance System Specification

## 1. Member Registration and Access Control

### Member Registration
- **Registration is OPEN to everyone** - Anyone can register as a member
- Registration endpoint: `/api/auth/register` (public, no authentication required)
- New members are automatically assigned the "Member" role
- Admins can also register new members through the same registration process

### Permission-Based Access Control
- **Permission system is separate from user registration**
- Permissions control **feature access**, not user creation
- Admins can always:
  - View all members
  - Edit member information
  - Delete/deactivate members
  - Manage member permissions
- Members can always:
  - Register themselves
  - Update their own profile
  - Make contributions
  - Request loans (if eligible)

### Admin Member Management
- Admins can view all members in "Manage Members" page
- Admins can edit member details (name, email, phone)
- Admins can delete/deactivate members
- Admins can grant/revoke permissions to members
- **Admins do NOT need special permission to add/manage members** - this is a core admin function

---

## 2. Contribution System - Fixed Amount Handling

### Fixed Amount Contributions

When an admin sets a **Fixed Amount** for a goal (e.g., $100 per contribution), the system follows these rules:

#### Option A: Strict Fixed Amount (Current Implementation)
- **Rule**: Contribution must be **exactly** the fixed amount
- **Behavior**: 
  - If user contributes $100 → ✅ Accepted
  - If user contributes $150 → ❌ Rejected with error: "Contribution amount must be exactly $100"
  - If user contributes $50 → ❌ Rejected with error: "Contribution amount must be exactly $100"

#### Option B: Fixed Amount with Excess Handling (Recommended for Community Finance)
- **Rule**: Contribution must be **at least** the fixed amount
- **Behavior**:
  - If user contributes $100 → ✅ Accepted as $100 contribution
  - If user contributes $150 → ✅ Accepted, but handled as:
    - **Option B1**: Full amount accepted ($150 total contribution)
    - **Option B2**: Split into fixed amount + excess ($100 fixed + $50 excess tracked separately)
    - **Option B3**: Fixed amount accepted, excess refunded/rejected ($100 accepted, $50 rejected)

### Recommended Implementation: Option B1 (Full Amount Accepted)

**Rationale for Community Finance:**
- Members may want to contribute more to help the community
- Excess contributions still benefit the goal
- Simpler system - no need to track excess separately
- More flexible for members

**Implementation:**
- Fixed amount = **minimum** contribution required
- Members can contribute more than the fixed amount
- Full amount counts toward:
  - Goal progress
  - User's contribution total
  - Loan eligibility tier

**Example:**
- Goal has fixed amount: $100
- Member contributes: $150
- Result: ✅ $150 is accepted and counted fully
- Goal progress increases by $150
- Member's total contributions increase by $150

---

## 3. Contribution Limits Priority Order

When multiple limits are set, they are checked in this order:

1. **Fixed Amount** (if set)
   - Must be at least the fixed amount (if using Option B)
   - Or exactly the fixed amount (if using Option A - strict)

2. **Minimum Amount** (if set, and no fixed amount)
   - Contribution must be at least this amount

3. **Maximum Amount Per Contribution** (if set)
   - Single contribution cannot exceed this amount

4. **Maximum Total Per User** (if set)
   - User's total contributions across all goals cannot exceed this

**Example:**
- Fixed Amount: $100
- Minimum Amount: $50
- Maximum Amount: $200
- Maximum Total Per User: $5,000

**User tries to contribute $150:**
- ✅ Passes Fixed Amount check ($150 >= $100)
- ✅ Passes Maximum Amount check ($150 <= $200)
- ✅ Passes Maximum Total check (if user's total + $150 <= $5,000)
- **Result**: Contribution accepted

---

## 4. Community Finance Best Practices

### Goal Management
- Set realistic fixed amounts based on member capacity
- Use fixed amounts for:
  - Regular savings goals (e.g., monthly $100 contributions)
  - Equal participation requirements
  - Structured savings plans

### Contribution Flexibility
- Allow members to contribute more than fixed amount (encourages community support)
- Track all contributions for transparency
- Provide clear feedback on contribution limits

### Member Management
- Keep registration open for community growth
- Use permissions to control feature access, not membership
- Admins maintain full control over member management

---

## 5. System Behavior Summary

| Scenario | Current Behavior | Recommended Behavior |
|----------|-----------------|---------------------|
| Fixed Amount = $100, User contributes $100 | ✅ Accepted | ✅ Accepted |
| Fixed Amount = $100, User contributes $150 | ❌ Rejected | ✅ Accepted (full $150) |
| Fixed Amount = $100, User contributes $50 | ❌ Rejected | ❌ Rejected (below minimum) |
| Admin adds member | ✅ Allowed | ✅ Allowed (always) |
| Member registers | ✅ Allowed | ✅ Allowed (public) |
| Permission-based access | Controls features | Controls features (not registration) |

---

## Next Steps

1. Update ContributionService to use "Fixed Amount = Minimum" instead of "Fixed Amount = Exact"
2. Update error messages to reflect new behavior
3. Update frontend to show fixed amount as "minimum required" instead of "exact amount"
4. Document the system behavior in user-facing help text


