
import java.io.DataOutputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.PrintStream;
import java.net.HttpURLConnection;
import java.net.Socket;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.Date;

import javax.sound.midi.SysexMessage;

public class Connections extends Thread {
	private Socket socket;
	private Out out;
	private In in;
	private String message;     // one line buffer
	static String screenName;
	
	static int hrmin = Integer.MAX_VALUE;
	static int hrmax = Integer.MIN_VALUE;
	static int hrsum = 0;
	static int hravg = 0;
	static int OXmin = Integer.MAX_VALUE;
	static int OXmax = Integer.MIN_VALUE;
	static int OXsum = 0;
	static int OXavg = 0;
	static int conncount =0;

	public Connections(Socket socket) throws FileNotFoundException {
		in  = new In(socket);
		out = new Out(socket);
		this.socket = socket;
		conncount++;
		System.err.println("Connection Count is: "+conncount);
		if(conncount > 6){
			System.err.println("Too many connections. Closing.");
			in.close();
			out.close();
			try {
				socket.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}

	}

	public void println(String s) { out.println(s); }

    public void run() {
		
		// waits for data and reads it in until connection dies
		// readLine() blocks until the server receives a new line from client
		String s;
		int hrinc = 0;
		int oxinc = 0;
		String str = "";
		//if flag then new file b/c new connection
		boolean flag = false;
		
		int[] hrArray= new int[1000];
		int[] oxArray= new int[1000];
		


        while ((s = in.readLine()) != null) {
        	
        
            setMessage(s);
        
 			String tstamp = new SimpleDateFormat("hh:mm:ss").format(new Date());
			//displays err output for testing
            
//            if(s.contains("|")){
//            	String[] parts = s.split("\\|");
//            	
//            	str = parts[1];
//            	screenName = parts[0];
//
//            	System.err.println("SENTBY: "+screenName+" str: "+str);
//            	
//            }
//        	if(!flag){
//        		flag = true;
//        		String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
//        		//screenName= EchoClient.name;
//        		//screenName = EchoClient.
//        		System.err.println("Name is: "+screenName);
//        		PrintStream fout;
//        		try {
//        			fout = new PrintStream(new FileOutputStream(screenName+" "+fname));
//        			System.setOut(fout);
//        		} catch (FileNotFoundException e2) {
//        			// TODO Auto-generated catch block
//        			e2.printStackTrace();
//        		}
//        	}
            
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
//		HRMath(hrArray, hrinc);
//		OXMath(oxArray,oxinc);
		
//		try {
//			send(screenName);
//		} catch (IOException e1) {
//			// TODO Auto-generated catch block
//			e1.printStackTrace();
//		}
		
        out.close();
        in.close();
        try                 { socket.close();      }
        catch (Exception e) { e.printStackTrace(); }
        System.err.println("closing socket");
    }
 
	public static void HRMath(int[] array,int length){

		if(length != 0){
			for(int i=0;i<length;i++){
				System.err.println("HRArray["+i+"]: "+array[i]);

				hrmin = Math.min(array[i], hrmin);
				hrmax = Math.max(array[i], hrmax);      

				hrsum = hrsum+array[i];

				//**
				//System.err.println("HRSUM: "+sum);
				//System.err.println("HRarray.length: "+length);
			}
			System.out.println("_________________");
			if(hrsum != 0){
				hravg = hrsum/length;
			}

			System.out.println("HRmax: "+hrmax);
			System.out.println("HRmin: "+hrmin);
			System.out.println("HRavg: "+hravg);
		}else{
			System.out.println("Heartrate data finished collecting.");
			//System.err.println("No heartrate data was collected.");
		}


	}
	public static void OXMath(int[] array,int length){

		if(length !=0){
			for(int i=0;i<length;i++){
				System.err.println("OXArray["+i+"]: "+array[i]);

				OXmin = Math.min(array[i], OXmin);
				OXmax = Math.max(array[i], OXmax);      

				OXsum = OXsum+array[i];

				//**
				//System.err.println("OXSUM: "+sum);
				//System.err.println("OXarray.length: "+length);
			}
			System.out.println("_________________");
			if(OXsum != 0){
				OXavg = OXsum/length;
			}

			System.out.println("OXmax: "+OXmax);
			System.out.println("OXmin: "+OXmin);
			System.out.println("OXavg: "+OXavg);
		}else{
			System.out.println("O2Sat data finished collecting.");
			//System.err.println("No O2Sat data was collected.");
		}

	}


	/*********************************************************************
	 *  The methods getMessage() and setMessage() are synchronized
	 *  so that the thread in Connection doesn't call setMessage()
	 *  while the ConnectionListener thread is calling getMessage().
	 *********************************************************************/
	public synchronized String getMessage() {
		if (message == null) return null;
		String temp = message;
		message = null;
		notifyAll();
		return temp;
	}


	public synchronized void setMessage(String s) {
		if (message != null) {
			try                  { wait();               }
			catch (Exception ex) { ex.printStackTrace(); }
		}
		message = s;
	}
    // listen to socket and print everything that server broadcasts
	
	 

	
    public static void send(String screenName) throws IOException{
    	System.err.println("In Send Method.");
    	
    	// URL goes here
    	String request = "http://www.google.ca/search?q=brian";
    	//String request = "blah";
    	if(!request.matches("blah")){
    	URL url = new URL(request); 
    	HttpURLConnection connection = (HttpURLConnection) url.openConnection();           
    	connection.setDoOutput(true);
    	connection.setDoInput(true);
    	connection.setInstanceFollowRedirects(false); 
    	connection.setRequestMethod("POST"); 
    	connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded"); 
    	connection.setRequestProperty("charset", "utf-8");
    	connection.setUseCaches (false);
    	

    	DataOutputStream wr = new DataOutputStream(connection.getOutputStream ());
    	//System.err.println(screenName+"'s OXavg: "+OXavg);
    	String content = "name:"+screenName+" OXavg:"+OXavg+" OXmin:"+OXmin+" OXmax:"+OXmax+" hravg:"+hravg+" hrmin:"+
    	hrmin+" hrmax: "+hrmax;
    	wr.writeBytes(content);
    	wr.flush();
    	wr.close();
    	
    	int responseCode = connection.getResponseCode();
    	System.err.println("Sending 'POST' request to URL : "+ url);
    	System.err.println("Post content: "+content);
    	System.err.println("Response code: "+responseCode);
    	connection.disconnect();
    	}
    }

}
