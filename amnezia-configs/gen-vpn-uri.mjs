// Builds an AmneziaVPN vpn:// import link from a client .conf.
// Encoding mirrors github.com/arshammi/awg-vpn-uri (4-byte BE length + zlib JSON, base64url).
import fs from "node:fs";
import zlib from "node:zlib";

const file = process.argv[2] || "phone-awg.conf";
const desc = process.argv[3] || "Phone";
const conf = fs.readFileSync(file, "utf8").trim();

const kv = {};
for (const raw of conf.split("\n")) {
  const line = raw.trim();
  if (!line || line.startsWith("#") || line.startsWith(";") || line.startsWith("[")) continue;
  const i = line.indexOf("=");
  if (i < 0) continue;
  kv[line.slice(0, i).trim()] = line.slice(i + 1).trim();
}
const put = (o, k, v) => {
  v = (v ?? "").trim();
  if (v !== "") o[k] = v;
};
const csv = (s) => (s || "").split(",").map((x) => x.trim()).filter(Boolean);

if (!kv.PrivateKey || !kv.PublicKey) throw new Error("missing PrivateKey/PublicKey");
const ep = kv.Endpoint || "";
const [host, portStr] = ep.split(":");
const port = Number(portStr);
if (!host || !port) throw new Error("bad Endpoint");

const dns = csv(kv.DNS);
const client = {};
put(client, "config", conf);
put(client, "hostName", host);
client["port"] = port;
put(client, "client_ip", csv(kv.Address)[0]);
put(client, "client_priv_key", kv.PrivateKey);
put(client, "server_pub_key", kv.PublicKey);
put(client, "psk_key", kv.PresharedKey);
put(client, "mtu", kv.MTU);
put(client, "persistent_keep_alive", kv.PersistentKeepalive);
client["allowed_ips"] = csv(kv.AllowedIPs).length ? csv(kv.AllowedIPs) : ["0.0.0.0/0", "::/0"];

for (const k of [
  "Jc", "Jmin", "Jmax", "S1", "S2", "S3", "S4",
  "H1", "H2", "H3", "H4", "I1", "I2", "I3", "I4", "I5",
  "HeaderProtectionKey", "ContentPaddingAddition",
  "RekeyAfterTime", "RekeyTimeout", "RejectAfterTime",
  "KeepaliveTimeout", "MaxHandshakeAttempts",
]) put(client, k, kv[k]);

const awg = {
  isThirdPartyConfig: true,
  last_config: JSON.stringify(client),
  port: String(port),
  transport_proto: "udp",
};
const awg3 = ["HeaderProtectionKey", "ContentPaddingAddition", "RekeyAfterTime",
  "RekeyTimeout", "RejectAfterTime", "KeepaliveTimeout", "MaxHandshakeAttempts"]
  .some((k) => (kv[k] || "").trim() !== "");
if (awg3) awg["protocol_version"] = "3";

const outer = {
  containers: [{ awg, container: "amnezia-awg" }],
  defaultContainer: "amnezia-awg",
  description: desc,
  dns1: dns[0] || "1.1.1.1",
  dns2: dns[1] || "1.0.0.1",
  hostName: host,
};

const json = Buffer.from(JSON.stringify(outer), "utf8");
const z = zlib.deflateSync(json, { level: 9 });
const payload = Buffer.alloc(4 + z.length);
payload.writeUInt32BE(json.length, 0);
z.copy(payload, 4);
const uri = "vpn://" + payload.toString("base64url");

process.stdout.write(uri + "\n");

// self-check: decode back and confirm round-trip
const back = zlib.inflateSync(payload.subarray(4));
const ok = back.length === json.length && back.equals(json);
process.stderr.write(`roundtrip=${ok} json=${json.length}B uri=${uri.length}B\n`);
