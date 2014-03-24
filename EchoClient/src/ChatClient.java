

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
        String tstamp = new SimpleDateFormat("hh:mm:ss").format(new Date());
		String fname = new SimpleDateFormat("yyyy-MM-dd hh-mm-ss'.txt'").format(new Date());
		PrintStream fout = new PrintStream(new FileOutputStream("Patient "+fname));
		
		System.setOut(fout);
			
		
        while ((s = in.readLine()) != null) {
           // enteredText.insert(s + "\n", enteredText.getText().length());
           // enteredText.setCaretPosition(enteredText.getText().length());
            
			//parses for 'HR' prefix. Records and prints HR data
			if (s.contains("HR")){
				String[] hrStr = s.split("HR ");

				StringBuilder builder = new StringBuilder();
				for(String t : hrStr) {
					builder.append(t);
				}

			//	System.err.println("HR "+builder.toString()+"   |"+tstamp);
				System.out.println("HR "+builder.toString()+"  |"+tstamp);

				
				int hrval = Integer.parseInt(builder.toString());
				//numbers[i] = Integer.parseInt(hrStr[i]);
				
				// **
				//System.err.println("Number is: "+hrval+" Inc is: "+hrinc);
				
				// Heartrate value of 0should usually be ignored since it it likely 
				// attributed to equipment issues and not death.
				if(hrval !=0){
				hrArray[hrinc] = hrval;
				hrinc++;
				System.err.println("hrnum: "+hrval);
				System.err.println("hrinc: "+hrinc);
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
				//System.err.println("OX "+builder.toString()+"|"+tstamp);
				System.out.println("OX "+builder.toString()+"|"+tstamp);

				//int oxval = Integer.parseInt(builder.toString());
				
				//**
				//System.err.println("Number is: "+oxNum+" Inc is: "+oxinc);
				oxArray[oxinc] = oxNum;
				oxinc++;
				System.err.println("oxnum: "+oxNum);
				System.err.println("oxinc: "+oxinc);
			}
			
        }
//        System.err.println("**:"+hrArray[1]);
//        System.err.println("**inc:"+hrinc);
//        System.err.println("**:"+oxArray[1]);
//        System.err.println("**:"+oxinc);
        // Add Call to HR math and OX MATH?
        

        out.close();
        in.close();
        try                 { socket.close();      }
        catch (Exception e) { e.printStackTrace(); }
        System.err.println("Closed client socket");
		
		
    }
    public static void HRMath(int[] array,int length){
		int hrmin = Integer.MAX_VALUE;
		int hrmax = Integer.MIN_VALUE;
		int sum = 0;
		int hravg = 0;
		System.err.println("IN HRMATH");
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
		System.err.println("IN OXMATH");
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
    public static void main(String[] args)  {
        ChatClient client = new ChatClient(args[0], args[1]);
        try {
			client.listen();
		} catch (FileNotFoundException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
        System.err.println("HR stuff: "+hrArray[1]+" "+hrinc);
        System.err.println("OX stuff: "+oxArray[1]+" "+oxinc);
        HRMath(hrArray, hrinc);
		OXMath(oxArray,oxinc);
    }
}
