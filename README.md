# 🏦 Digital Banking System (Fullstack)

A secure fullstack banking application built using **ASP.NET Core Web API** and **Next.js (React)**.
This system supports authentication, account management, and financial transactions with **JWT security and ACID compliance**.

---

## 🚀 Tech Stack

### 🔧 Backend

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* JWT Authentication
* BCrypt Password Hashing

### 🎨 Frontend

* Next.js (App Router)
* React.js
* Axios
* Tailwind CSS

---

## 🔐 Features

### ✅ Authentication

* User Registration
* User Login
* Password hashing with BCrypt
* JWT Token-based authentication
* Secure API access

---

### 💳 Account Management

* Create bank account
* View user accounts
* Unique account number generation
* Balance tracking

---

### 💸 Transactions

#### 🔁 Transfer Money

* Uses **ACID transactions**
* Prevents partial updates
* Validations:

  * Ownership check
  * Balance check
  * Account existence

#### 💰 Deposit

* Adds funds to account
* Stores transaction record

#### 💵 Withdraw

* Deducts balance
* Prevents overdraft

---

### 📜 Transaction History

* View all transactions
* Includes:

  * From account
  * To account
  * Amount
  * Type

---

## 🔐 Authentication Flow

1. User logs in via `/api/auth/login`
2. Backend returns JWT token
3. Token stored in browser (localStorage)
4. Token sent in headers:

```
Authorization: Bearer YOUR_TOKEN
```

5. Protected APIs can now be accessed

---

## 🧠 Key Concepts Implemented

### 🔑 JWT Authentication

* Secure endpoints
* Claims-based identity
* Includes:

  * UserId (NameIdentifier)
  * Email
  * Role

---

### ⚙️ ACID Transactions

Ensures:

* Atomicity
* Consistency
* Isolation
* Durability

Implemented using:

```csharp
await _context.Database.BeginTransactionAsync();
```

---

### 🗄️ Database Structure

#### Users

* Id
* Name
* Email
* PasswordHash
* Role

#### Accounts

* Id
* UserId
* AccountNumber
* Balance
* AccountType

#### Transactions

* Id
* FromAccountId
* ToAccountId
* Amount
* Type

---

## 🧪 Testing Flow

1. Register user
2. Login → receive JWT token
3. Authorize API using token
4. Create account
5. Deposit money
6. Transfer money
7. Withdraw money
8. View transaction history

---

## ⚠️ Issues Faced & Fixes

### ❌ 401 Unauthorized

✔ Fixed by adding JWT token in headers

---

### ❌ Auto Logout After Login

✔ Fixed by adding `NameIdentifier` claim in JWT

---

### ❌ Empty Account Data

✔ Fixed backend filtering using UserId from JWT

---

### ❌ JSON Cycle Error

✔ Fixed using:

```csharp
ReferenceHandler.IgnoreCycles
```

---

### ❌ CORS Error

✔ Enabled CORS in backend for frontend access

---

## 🔗 API Endpoints

### 🔐 Auth

* POST `/api/auth/register`
* POST `/api/auth/login`

### 💳 Account

* POST `/api/account/create`
* GET `/api/account`

### 💸 Transactions

* POST `/api/transaction/transfer`
* POST `/api/transaction/deposit`
* POST `/api/transaction/withdraw`
* GET `/api/transaction/history`

---

## 🖥️ Frontend Pages

* `/login` → User login
* `/dashboard` → View accounts
* `/transfer` → Send money
* `/history` → View transactions

---

## 🚀 Current Status

✔ Backend fully functional
✔ Frontend connected to backend
✔ JWT authentication working
✔ Account system working
✔ Transactions working
✔ Dashboard displaying real data

---

## 🔥 Future Improvements

* Add Deposit & Withdraw UI
* Add Navbar & Protected Routes
* Add Account selection dropdown
* Improve UI/UX (cards, charts)
* Add email notifications
* Add logging (Serilog)

---

## 👨‍💻 Author

**Anil Kumar**
Fullstack Developer (React + .NET)
2+ years experience in web development & SaaS

---

## ⭐ Support

If you like this project, please ⭐ the repo!
