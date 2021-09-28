FROM node:14
WORKDIR /app
COPY ./ ./

RUN npm ci
RUN npm run build


FROM nginx
COPY --from=0 /app/dist /usr/share/nginx/html

