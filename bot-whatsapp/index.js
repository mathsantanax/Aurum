import express from "express";
import axios from "axios";
import dotenv from "dotenv";
import bodyParser from "body-parser";

dotenv.config();

const app = express();
app.use(bodyParser.json());

const token = process.env.WHATSAPP_TOKEN;
const phoneId = process.env.PHONE_NUMBER_ID;
const tokenVerify = process.env.VERIFY_WEBHOOK_TOKEN;


app.get("/webhook", (req, res) => {
    const mode = req.query["hub.mode"];
    const challenge = req.query["hub.challenge"];
    const verify_token = req.query["hub.verify_token"];

    if(mode && verify_token === tokenVerify){
        res.status(200).send(challenge);
        console.log("Verificação Feita");
    }
    else{
        res.status(403);
    }
});

app.post("/webhook", async (req, res) => {
    
    const message = req.body.entry?.[0]?.changes?.[0]?.value?.messages?.[0];

    if(message){
        const from = message.from;
        const text = message.text?.body;
        console.log("Mensagem de: ", from  + "\nCorpo da Mensagem: \n", text);
        if (text?.toLowerCase() === "oi") {
            try {
                await axios.post(
                `https://graph.facebook.com/v22.0/${phoneId}/messages`,
                {
                    messaging_product: "whatsapp",
                    to: from,
                    text: { body: "Hello, sent through the created API" },
                },
                {
                    headers: { Authorization: `Bearer ${token}` },
                    "Content-Type": "application/json"
                }
            );
            } catch (error) {
                console.error("Error sending message:", error.response?.data || error.message);
            }
        }
        else if(text?.toLowerCase() === "bot")
        {
            try {
                await axios.post(
                `https://graph.facebook.com/v22.0/${phoneId}/messages`,
                {
                    messaging_product: "whatsapp",
                    to: from,
                    text: { body: "Esse bot foi feito para o aplicativo Aurum Finance, Criado pela empresa Anlumina! \n bem-vindo" },
                },
                {
                    headers: { Authorization: `Bearer ${token}` },
                    "Content-Type": "application/json"
                }
            );
            } catch (error) {
                console.error("Error sending message:", error.response?.data || error.message);
            }
        }
    }
    res.sendStatus(200);
});

app.get("/", (req, res) => {
    res.send("Bot on");
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`Servidor ouvindo na porta ${PORT}`);
});

