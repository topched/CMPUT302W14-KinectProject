
/*************************************************************************
 *  Compilation:  javac EchoServer.java
 *  Execution:    java EchoServer port
 *  Dependencies: In.java Out.java
 *  
 *  Runs an echo server which listens for connections on port 4444,
 *  and echoes back whatever is sent to it.
 *
 *	Lines directly under comments starting with //** are for testing and should
 *  be removed or commented out.
 *
 *  % java EchoServer 4444
 *
 *
 *  Limitations
 *  -----------
 *  The server is not multi-threaded, so at most one client can connect
 *  at a time.
 *
 *************************************************************************/


import java.net.Socket;
import java.net.ServerSocket;
import java.io.*;
import java.text.SimpleDateFormat;
import java.util.Collection;
import java.util.Date;

public class EchoServer {

	public static void main(String[] args) throws Exception {

		// create socket
		int port = 4444;
		ServerSocket serverSocket = new ServerSocket(port);
		System.err.println("Started server on port " + port);

		//sets output stream to fout which will be appended to (date/time).txt
		//may want to add user name in the future
		String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
		String tstamp = new SimpleDateFormat("hh:mm:ss").format(new Date());
		PrintStream fout = new PrintStream(new FileOutputStream(fname));
		System.setOut(fout);

		// repeatedly wait for connections, and process
		while (true) {

			// a "blocking" call which waits until a connection is requested
			Socket clientSocket = serverSocket.accept();
			System.err.println("Accepted connection from client");

			// open up IO streams
			In  in  = new In (clientSocket);
			Out out = new Out(clientSocket);

			// waits for data and reads it in until connection dies
			// readLine() blocks until the server receives a new line from client
			String s;
			int hrinc = 0;
			int oxinc = 0;

			//if flag then OX, if flag = 0 then HR
			int[] hrArray= new int[1000];
			int[] oxArray= new int[1000];
			while ((s = in.readLine()) != null) {
				out.println(s);

				//displays err output for testing
				// System.err.println("Line is: "+s+" "+tstamp);

				//parses for 'HR' prefix. Records and prints HR data
				if (s.contains("HR")){
					String[] hrStr = s.split("HR ");



					StringBuilder builder = new StringBuilder();
					for(String t : hrStr) {
						builder.append(t);
					}

					System.err.println("HR "+builder.toString()+"   |"+tstamp);
					System.out.println("HR "+builder.toString()+"   |"+tstamp);

					
					int hrval = Integer.parseInt(builder.toString());
					//numbers[i] = Integer.parseInt(hrStr[i]);
					
					// **
					//System.err.println("Number is: "+hrval+" Inc is: "+hrinc);
					
					// Heartrate value of 0should usually be ignored since it it likely 
					// attributed to equipment issues and not death.
					if(hrval !=0){
					hrArray[hrinc] = hrval;
					hrinc++;
					}
				}
				//parses for 'OX' prefix. Records and prints OX data.
				if (s.contains("OX")){
					String[] oxStr = s.split("OX "); 


					StringBuilder builder = new StringBuilder();
					for(String t : oxStr) {
						builder.append(t);
					}
					String[] split= builder.toString().split("\\s+");
					String first = split[0];
					int oxNum = Integer.parseInt(first);
					System.err.println("OX "+builder.toString()+"|"+tstamp);
					System.out.println("OX "+builder.toString()+"|"+tstamp);

					//int oxval = Integer.parseInt(builder.toString());
					
					//**
					//System.err.println("Number is: "+oxNum+" Inc is: "+oxinc);
					oxArray[oxinc] = oxNum;
					oxinc++;

				}

			}
			HRMath(hrArray, hrinc);
			OXMath(oxArray,oxinc);

			// close IO streams, then socket
			System.err.println("Closing connection with client");
			out.close();
			in.close();
			clientSocket.close();
		}
	}
	public static void HRMath(int[] array,int length){
		int hrmin = Integer.MAX_VALUE;
		int hrmax = Integer.MIN_VALUE;
		int sum = 0;
		int hravg = 0;

		if(length != 0){
			for(int i=0;i<length;i++){
				System.err.println("HRArray["+i+"]: "+array[i]);

				hrmin = Math.min(array[i], hrmin);
				hrmax = Math.max(array[i], hrmax);      

				sum = sum+array[i];
				
				//**
				//System.err.println("HRSUM: "+sum);
				//System.err.println("HRarray.length: "+length);
			}
			System.out.println("_________________");
			if(sum != 0){
				hravg = sum/length;
			}
			
			System.out.println("HRmax: "+hrmax);
			System.out.println("HRmin: "+hrmin);
			System.out.println("HRavg: "+hravg);
		}else{
			System.out.println("No heartrate data was collected.");
			System.err.println("No heartrate data was collected.");
		}


	}
	public static void OXMath(int[] array,int length){
		int OXmin = Integer.MAX_VALUE;
		int OXmax = Integer.MIN_VALUE;
		int sum = 0;
		int OXavg = 0;
		if(length !=0){
			for(int i=0;i<length;i++){
				System.err.println("OXArray["+i+"]: "+array[i]);

				OXmin = Math.min(array[i], OXmin);
				OXmax = Math.max(array[i], OXmax);      

				sum = sum+array[i];
				
				//**
				//System.err.println("OXSUM: "+sum);
				//System.err.println("OXarray.length: "+length);
			}
			System.out.println("_________________");
			if(sum != 0){
				OXavg = sum/length;
			}
			
			System.out.println("OXmax: "+OXmax);
			System.out.println("OXmin: "+OXmin);
			System.out.println("OXavg: "+OXavg);
		}else{
			System.out.println("No O2Sat data was collected.");
			System.err.println("No O2Sat data was collected.");
		}
	}


}
