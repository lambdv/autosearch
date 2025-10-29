#use official redis image
FROM redis:7-alpine

#expose default redis port
EXPOSE 6379

#persist data to /data inside the container
VOLUME ["/data"]

#basic health check
HEALTHCHECK --interval=10s --timeout=3s --retries=3 CMD redis-cli ping || exit 1

#start redis with appendonly persistence and sane defaults
CMD ["redis-server", "--appendonly", "yes", "--save", "20", "1", "--loglevel", "warning"]

