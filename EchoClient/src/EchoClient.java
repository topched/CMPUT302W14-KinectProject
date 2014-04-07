
/*************************************************************************
 *  Compilation:  javac EchoClient.java In.java Out.java
 *  Execution:    java EchoClient name host
 *
 *  Connects to host server on port 4444, sends text, and prints out
 *  whatever the server sends back.
 *
 *  
 *  % java EchoClient wayne localhost
 *  Connected to localhost on port 4444
 *  this is a test
 *  [wayne]: this is a test
 *  it works
 *  [wayne]: it works
 *  <Ctrl-d>                 
 *  Closing connection to localhost
 *
 *  Windows users: replace <Ctrl-d> with <Ctrl-z>
 *  
 *************************************************************************/

import java.io.DataOutputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;
import java.net.HttpURLConnection;
import java.net.Socket;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.Date;

import javax.sound.midi.SysexMessage;


public class EchoClient {
	public static String name;
	static int hrmin = Integer.MAX_VALUE;
	static int hrmax = Integer.MIN_VALUE;
	static int hrsum = 0;
	static int hravg = 0;
	static int OXmin = Integer.MAX_VALUE;
	static int OXmax = Integer.MIN_VALUE;
	static int OXsum = 0;
	static int OXavg = 0;

    public static void main(String[] args) throws Exception {
    	String screenName = args[0];
        String host       = args[1];
        int port          = 4444;
        name = args[0];
        String name2;
		int hrinc = 0;
		int oxinc = 0;
		String str = "";
		//if flag then new file b/c new connection
		boolean flag = false;
		
		int[] hrArray= new int[1000];
		int[] oxArray= new int[1000];
		
        // connect to server and open up IO streams
        Socket socket = new Socket(host, port);
        In     stdin  = new In();
        In     in     = new In(socket);
        Out    out    = new Out(socket);
        System.err.println("Connected to " + host + " on port " + port);
        
        OutputStream os = socket.getOutputStream();
        
       // setName(screenName);
        //name2 = getName();
        
        // read in a line from stdin, send to server, and print back reply
        while (stdin.hasNextLine()) {
        	

            // read line of client
            String s = stdin.readLine();

            // Sending
            String toSend = s;
            byte[] toSendBytes = toSend.getBytes();
            int toSendLen = toSendBytes.length;
            byte[] toSendLenBytes = new byte[2];
            toSendLenBytes[0] = (byte)(toSendLen & 0xff);
            toSendLenBytes[1] = (byte)((toSendLen >> 8) & 0xff);
            //toSendLenBytes[2] = (byte)((toSendLen >> 16) & //0xff);
            //toSendLenBytes[3] = (byte)((toSendLen >> 24) & //0xff);
            //os.write(toSendLenBytes);
            //os.write(toSendBytes);
            //System.err.println("Byte is: "+toSendBytes.toString());
            
 			String tstamp = new SimpleDateFormat("hh:mm:ss").format(new Date());
			//displays err output for testing
            //System.out.println("S is: "+s);
            //System.err.println("Se is: "+s);
 				
            // send over socket to server
 			out.println(screenName+"|" + s);
            if(s.contains("|")){
            	String[] parts = s.split("\\|");
            	
            	str = parts[1];
            	screenName = parts[0];

            	System.err.println("SENTBY: "+screenName+" str: "+str);
            	
            }
        	if(!flag){
        		flag = true;
        		String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
        		//screenName= EchoClient.name;
        		//screenName = EchoClient.
        		//System.err.println("nameis: "+screenName);
        		PrintStream fout;
        		try {
        			fout = new PrintStream(new FileOutputStream(screenName+fname));
        			System.setOut(fout);
        		} catch (FileNotFoundException e2) {
        			// TODO Auto-generated catch block
        			e2.printStackTrace();
        		}
        	}
            
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
            // get reply from server and print it out
            if(s.equals("HR 666")){
                System.err.println("Closing connection to " + host);
        		HRMath(hrArray, hrinc);
        		OXMath(oxArray,oxinc);
        		try {
        			send(screenName);
        		} catch (IOException e1) {
        			// TODO Auto-generated catch block
        			e1.printStackTrace();
        		}
                out.close();
                in.close();
                socket.close();
            }
        }


            

        
        // close IO streams, then socket
        System.err.println("Closing connection to " + host);
        out.close();
        in.close();
        socket.close();
    }



	EchoClient(String screenName){
	this.name = screenName;
}

     public static String getName(){
        return name;
     }
     public static void setName(String sname){
    	 name = sname;
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
    public static void send(String screenName) throws IOException{
    	System.err.println("In Send Method.");
    	
    	// URL goes here
    	String request = "~/Documents/workspace/EchoClient/src/a.html";
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