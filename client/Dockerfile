FROM node:18-alpine AS builder
WORKDIR /app
COPY package.json .
COPY package-lock.json .
RUN npm install 

# ARG REACT_APP_API_URL=http://asteroids_signalr:6000
# ARG REACT_APP_AUTHORITY=https://harnesskc.duckdns.org:25651/realms/harness
# ARG REACT_APP_CLIENT_ID=asteroids

# ENV REACT_APP_SSO_URL=$REACT_APP_API_URL
# ENV REACT_APP_AUTHORITY=$REACT_APP_AUTHORITY
# ENV REACT_APP_CLIENT_ID=$REACT_APP_CLIENT_ID

COPY . .
RUN npm run build

FROM nginx:alpine
RUN rm -rf /usr/share/nginx/html/*
COPY --from=builder /app/dist /usr/share/nginx/html
COPY default.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]