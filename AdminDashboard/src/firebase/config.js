import { initializeApp } from "firebase/app";
import { getFirestore } from "firebase/firestore";

// TODO: 개발자님께서 유니티 프로젝트에 사용된 Firebase 구성을 여기에 복사하여 붙여넣으셔야 작동합니다!
const firebaseConfig = {
  apiKey: "AIzaSyCxIbJYanZQahmGDYgUNNa9H21AFhMs4eo",
  authDomain: "legendguardian-d1c51.firebaseapp.com",
  databaseURL: "https://legendguardian-d1c51-default-rtdb.firebaseio.com",
  projectId: "legendguardian-d1c51",
  storageBucket: "legendguardian-d1c51.firebasestorage.app",
  messagingSenderId: "1019457959885",
  appId: "1:1019457959885:web:f4b410eb1321e8838f3684",
  measurementId: "G-DKR7GZKQ7L"
};

const app = initializeApp(firebaseConfig);
export const db = getFirestore(app);
