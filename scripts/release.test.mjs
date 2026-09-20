import {test} from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,writeFile,readFile,rm,chmod} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import path from 'node:path';
import {spawnSync} from 'node:child_process';
const script=path.resolve('scripts/package-release.sh');
const versionScript=path.resolve('scripts/check-release-version.mjs');
async function fixture(fn){const dir=await mkdtemp(path.join(tmpdir(),'ember-release-'));try{await fn(dir);}finally{await rm(dir,{recursive:true,force:true});}}
test('release tag must match serialized player version',()=>fixture(async dir=>{
  const settings=path.join(dir,'settings.asset');await writeFile(settings,'PlayerSettings:\n  bundleVersion: 1.2.3\n');
  for(const [tag,code] of [['v1.2.3',0],['v1.2.4',1],['main',1],['v1.2.3/evil',1]]){
    const r=spawnSync(process.execPath,[versionScript,tag,settings]);assert.equal(r.status,code,tag);
  }
}));
test('macOS archive preserves app executable permissions',()=>fixture(async dir=>{
  const root=path.join(dir,'macos');await mkdir(path.join(root,'Ember.app/Contents/MacOS'),{recursive:true});
  const binary=path.join(root,'Ember.app/Contents/MacOS/EMBER - Return to Earth');await writeFile(binary,'app');await chmod(binary,0o755);
  await writeFile(path.join(root,'Ember.app/Contents/Info.plist'),'plist');
  const r=spawnSync('bash',[script,'macos',root,path.join(dir,'dist')]);assert.equal(r.status,0,r.stderr?.toString());
  const tar=path.join(dir,'dist/Ember-macOS.tar.gz');
  const listing=spawnSync('tar',['-tvf',tar],{encoding:'utf8'});assert.match(listing.stdout,/-rwxr-xr-x.*Ember.app\/Contents\/MacOS\/EMBER - Return to Earth/);
}));
test('Windows archive includes executable and runtime data',()=>fixture(async dir=>{
  const root=path.join(dir,'windows');await mkdir(path.join(root,'Ember_Data'),{recursive:true});
  await writeFile(path.join(root,'Ember.exe'),'exe');await writeFile(path.join(root,'UnityPlayer.dll'),'dll');
  await writeFile(path.join(root,'Ember_Data/globalgamemanagers'),'data');
  const r=spawnSync('bash',[script,'windows',root,path.join(dir,'dist')]);assert.equal(r.status,0,r.stderr?.toString());
  const listing=spawnSync('unzip',['-Z1',path.join(dir,'dist/Ember-Windows-x64.zip')],{encoding:'utf8'});
  assert.match(listing.stdout,/UnityPlayer.dll/);assert.match(listing.stdout,/Ember_Data\/globalgamemanagers/);
}));
test('Linux archive contains executable and player data',()=>fixture(async dir=>{
  const root=path.join(dir,'linux');await mkdir(path.join(root,'Ember_Data'),{recursive:true});
  await writeFile(path.join(root,'Ember.x86_64'),'elf');await chmod(path.join(root,'Ember.x86_64'),0o755);await writeFile(path.join(root,'UnityPlayer.so'),'so');
  const r=spawnSync('bash',[script,'linux',root,path.join(dir,'dist')]);assert.equal(r.status,0,r.stderr?.toString());
  const listing=spawnSync('tar',['-tvf',path.join(dir,'dist/Ember-Linux-x64.tar.gz')],{encoding:'utf8'});
  assert.match(listing.stdout,/-rwxr-xr-x.*Ember.x86_64/);assert.match(listing.stdout,/UnityPlayer.so/);
}));
test('packager rejects incomplete builds and unknown platforms',()=>fixture(async dir=>{
  await mkdir(path.join(dir,'input'));await writeFile(path.join(dir,'input/Ember.exe'),'exe');
  for(const platform of ['macos','linux','windows','unknown']){
    const r=spawnSync('bash',[script,platform,path.join(dir,'input'),path.join(dir,'dist')]);assert.notEqual(r.status,0,platform);
  }
}));

test('WebGL archive has index at root and rejects invalid bundles',()=>fixture(async dir=>{
 const root=path.join(dir,'webgl');await mkdir(path.join(root,'Build'),{recursive:true});
 const zlib=await import('node:zlib');
 const names=['game.data.unityweb','game.framework.js.unityweb','game.wasm.unityweb','game.loader.js'];
 await writeFile(path.join(root,'index.html'),names.join('\n'));
 for(const n of names)await writeFile(path.join(root,'Build',n),n.endsWith('unityweb')?zlib.gzipSync(n.includes('wasm')?Buffer.from([0,97,115,109,1,0,0,0]):Buffer.alloc(32)):Buffer.alloc(32));
 const out=path.join(dir,'dist');assert.equal(spawnSync('bash',[script,'webgl',root,out]).status,0);
 const listing=spawnSync('unzip',['-Z1',path.join(out,'Ember-WebGL.zip')],{encoding:'utf8'}).stdout;assert.match(listing,/^index.html$/m);assert.match(listing,/^Build\/game.wasm.unityweb$/m);
 await rm(path.join(root,'Build/game.wasm.unityweb'));assert.notEqual(spawnSync('bash',[script,'webgl',root,out]).status,0);
}));

test('Unix packaging rejects players without execute permission',()=>fixture(async dir=>{
 const root=path.join(dir,'linux');await mkdir(path.join(root,'Ember_Data'),{recursive:true});
 await writeFile(path.join(root,'Ember.x86_64'),'elf');await chmod(path.join(root,'Ember.x86_64'),0o644);await writeFile(path.join(root,'UnityPlayer.so'),'so');
 assert.notEqual(spawnSync('bash',[script,'linux',root,path.join(dir,'dist')]).status,0);
}));
