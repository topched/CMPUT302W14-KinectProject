

/*************************************************************************
 *  Compilation:  javac ChatClient.java
 *  Execution:    java ChatClient name host
 *  Dependencies: In.java Out.java
 *
 *  Connects to host server on port 4444, enables an interactive
 *  chat client.
 *  
 *  % java ChatClient alice localhost
 *
 *  % java ChatClient bob localhost
 *  
 *************************************************************************/


import java.awt.*;
import java.awt.event.*;

import javax.swing.JFrame;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.JTextField;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.PrintStream;
import java.net.Socket;
import java.text.SimpleDateFormat;
import java.util.Date;

public class ChatClient extends JFrame {

    private String screenName;
    static int hrinc ;
	static int oxinc ;
	private String[] users={"","","",""};

	//if flag then OX, if flag = 0 then HR
	static int[] hrArray= new int[1000];
	static int[] oxArray= new int[1000];

    // GUI stuff
    //private JTextArea  enteredText = new JTextArea(10, 32);
    //private JTextField typedText   = new JTextField(32);

    // socket for connection to chat server
    private Socket socket;

    // for writing to and reading from the server
    private Out out;
    private In in;

    public ChatClient(String screenName, String hostName) {

        // connect to server
        try {
            socket = new Socket(hostName, 4444);
            out    = new Out(socket);
            in     = new In(socket);
        }
        catch (Exception ex) { ex.printStackTrace(); }
        //check if screenname equals
        this.screenName = screenName;

        
        // close output stream  - this will cause listen() to stop and exit
        addWindowListener(
            new WindowAdapter() {
                public void windowClosing(WindowEvent e) {
                    out.close();
//                    in.close();
//                    try                   { socket.close();        }
//                    catch (Exception ioe) { ioe.printStackTrace(); }
                }
            }
        );

    }


    // listen to socket and print everything that server broadcasts
    public void listen() throws FileNotFoundException {
        String s;
        String str ="";
        boolean flag = false;
        
		//String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
		//PrintStream hrout = new PrintStream(new FileOutputStream("PatientHR.txt "));
		//PrintStream oxout = new PrintStream(new FileOutputStream("PatientOX.txt "));
		// System.out saves to HR
		// System.err saves to OX
//		System.setOut(hrout);
//		System.setErr(oxout);
        boolean set = false;
        while ((s = in.readLine()) != null) {
           // enteredText.insert(s + "\n", enteredText.getText().length());
           // enteredText.setCaretPosition(enteredText.getText().length());
            
        // Add Call to HR math and OX MATH?
        	String tstamp = new SimpleDateFormat("hh:mm:ss").format(new Date());
        	 if(s.contains("|")){
             	String[] parts = s.split("\\|");
             	
             	str = parts[1];
             	screenName = parts[0];

             }

        	 // If user does not exist, create new file for them. 
        	 //System.err.println("Current user is: "+screenName);
        	 boolean exists = false;
        	 int l = 0;
        		 for(String k : users){
        			 if(k.toLowerCase().equals(screenName.toLowerCase())){
        				 exists = true;   				 
        			 }
        		 }
        		 
        		 
        		//flag = true;
        		//String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
        		//screenName= EchoClient.name;
        		//screenName = EchoClient.
        		//System.err.println("Name is: "+screenName);
         		PrintStream oxout;
         		PrintStream hrout;
        		//System.setOut(hrout);
        		//System.setErr(oxout);
         		
         		// Sets current user to one of the 4 possible name slots
        		
        		if(!exists){
           		while(!users[l].equals("")){
           			l++;	
           		}
           		users[l] = screenName;
        		try {
        			System.err.println("In try! Screenname: "+screenName+" 0: "+users[0]
        					+"1: "+users[1]+"2: "+users[2]+"3: "+users[3]);
        			//fout = new PrintStream(new FileOutputStream(screenName+" "+fname));
        			//System.setOut(fout);
        			hrout = new PrintStream(new FileOutputStream(screenName+" "+"HR.txt"));
        			System.setOut(hrout);
        			oxout = new PrintStream(new FileOutputStream(screenName+" "+"OX.txt"));
        			System.setErr(oxout);
        		} catch (FileNotFoundException e2) {
        			// TODO Auto-generated catch block
        			e2.printStackTrace();
        		}
        	}
        
 			//parses for 'HR' prefix. Records and prints HR data
 			if (s.contains("HR")){
 				String[] hrStr = str.split("HR ");

 				StringBuilder builder = new StringBuilder();
 				for(String t : hrStr) {
 					builder.append(t);
 				}

 				//System.err.println("HR "+builder.toString()+"   |"+tstamp);
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
 				String[] oxStr = str.split("OX "); 


 				StringBuilder builder = new StringBuilder();
 				for(String t : oxStr) {
 					builder.append(t);
 				}
 				String[] split= builder.toString().split("\\s+");
 				String first = split[0];
 				int oxNum = Integer.parseInt(first);
 				System.err.println("OX "+builder.toString()+"|"+tstamp);
 				//System.out.println("OX "+builder.toString()+"|"+tstamp);
 				
 				//int oxval = Integer.parseInt(builder.toString());
 				
 				//**
 				//System.err.println("Number is: "+oxNum+" Inc is: "+oxinc);
 				oxArray[oxinc] = oxNum;
 				oxinc++;

 			}
         }

        out.close();
        in.close();
        try                 { socket.close();      }
        catch (Exception e) { e.printStackTrace(); }
        //System.err.println("Closed client socket");
		
		
    }

    public static void main(String[] args)  {
        ChatClient client = new ChatClient(args[0], args[1]);
        try {
			client.listen();
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
       // System.err.println("HR stuff: "+hrArray[1]+" "+hrinc);
       // System.err.println("OX stuff: "+oxArray[1]+" "+oxinc);

    }
}
