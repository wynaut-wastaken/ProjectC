function server_handle_setblock(buffer) {
	
	buffer_seek(buffer,buffer_seek_start,0);
	//check if from client
	var side = buffer_read(buffer,buffer_bool); 
	if(side != CLIENT_SIDE) return false;
	//discard header
	buffer_read(buffer,buffer_u8); 
	
	var chunkx,chunky,posx,posy, type;
	
	// read data
	type = buffer_read(buffer,buffer_u16);
	chunkx = buffer_read(buffer,buffer_u64);
	chunky = buffer_read(buffer,buffer_u8);
	posx = buffer_read(buffer,buffer_u8);
	posy = buffer_read(buffer,buffer_u8);
	
	SetTile(chunkx, chunky,posx,posy,type);
	
	var r_buffer = packet_set_block(SERVER_SIDE,chunkx,chunky,posx,posy,type);
	
	// send it back
	for (var i = 0; i < ds_list_size(socket_list); i++) {
		network_send_packet(ds_list_find_value(socket_list,i),r_buffer,buffer_tell(r_buffer));
	}
	buffer_delete(buffer);
}

function server_handle_ping(buffer) {
		
	buffer_seek(buffer,buffer_seek_start,0);
	//check if from server
	var side = buffer_read(buffer,buffer_bool); 
	if(side == SERVER_SIDE) return;
		
	var r_buffer = packet_ping(SERVER_SIDE);
	
	// send it back
	for (var i = 0; i < ds_list_size(socket_list); i++) {
		network_send_packet(ds_list_find_value(socket_list,i),r_buffer,buffer_tell(r_buffer));
	}
}