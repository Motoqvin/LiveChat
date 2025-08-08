import NextAuth from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";

const handler = NextAuth({
    providers: [
        CredentialsProvider({
            name: "Credentials",
            credentials: {
                username: { label: "Email", type: "email" },
                password: { label: "Password", type: "password" }
            },
            async authorize(credentials) {
                const res = await fetch("http://localhost:5250/login", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        Login: credentials?.username,
                        Password: credentials?.password,
                    }),
                });
                
                if (!res.ok) return null;
                const user = await res.json();
                console.log("User data:", user);

                return {
                    id: user.id,
                    accessToken: user.token,
                    name: user.username || user.email,
                    email: user.email
                };
            },
        }),
    ],
    callbacks: {
        async jwt({ token, user }: { token: any, user: any }) {
            if (user) {
                token.accessToken = user.accessToken;
                token.user = user;
            }
            return token;
        },
        async session({ session, token }: { session: any, token: any }) {
            session.accessToken = token.accessToken;
            session.user = token.user ?? session.user;
            return session;
        },
    },
    session: {
        strategy: "jwt",
    }
});

export { handler as GET, handler as POST };