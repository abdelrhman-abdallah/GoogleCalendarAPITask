# GoogleCalendarAPITask

# 📅 Google Calendar API Integration

A .NET Core Web API for seamless management of Google Calendar events with enhanced security features.

## ✨ Features

Event Operations: Easily add, delete, and retrieve events from Google Calendar.
Pagination: pagination for fetching events with customizable result sizes.
Security: Passwords are securely hashed with salts, and user data is stored in Firestore. JWT tokens provide secure access to Google Calendar.

## 🚀 Getting Started

Prerequisites
.NET Core SDK
Google Cloud Platform Project with Calendar API enabled
Firestore Database
Google Calendar API credentials (JSON file)

## 🛠️ Project Setup

Clone the repository.
Configure secrets.json and authFireCloud.json with your google credentials and fire store credentials.

## 🔒 Security Measures

Password Hashing: Passwords are salted and hashed for enhanced security.
Firestore Integration: User data is securely stored in Firestore.
JWT Authentication: Unique JWT tokens provide secure access to Google Calendar, with automatic token renewal.

## 📁 Project Structure

Controllers: API controllers for event management.
DTO: Data Transfer Objects for data exchange.
Models: Data models for users and events.
Service: Business logic and Google Calendar API integration.
Firebase: Firestore integration.
