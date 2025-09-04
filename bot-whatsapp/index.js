import express from "express";
import axios from "axios";
import dotenv from "dotenv";

dotenv.config();
const app = express();
app.use(express.json());

const token = process.env.WHATSAPP_TOKEN;
const phoneId = process.env.PHONE_NUMBER_ID;


